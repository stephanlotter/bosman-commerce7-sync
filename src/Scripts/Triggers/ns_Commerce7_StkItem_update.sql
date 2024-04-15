use BAWINE;
-- Drops the trigger if it already exists
drop trigger if exists ns_Commerce7_StkItem_update;
go

-- Trigger definition
create trigger ns_Commerce7_StkItem_update
on StkItem
after insert, update
as
begin
	-- Change History
	-- 2023-11-25 08:57: Trigger Created

	set nocount on;
	if TRIGGER_NESTLEVEL(@@procid) > 1
		return;

	if update(Qty_On_Hand)
		or update(QtyOnPO)
		or update(QtyOnSO)
		or update(ReservedQty)
		return;

	if not update(ubIIOnLineItem)
		and (select ubIIOnLineItem from inserted) = 0
		return;

	if not update(ItemActive)
		and (select ItemActive from inserted) = 0
		return;

	-- Check for any updated monitor columns
	if not update(Code)
		and not update(cSimpleCode)
		and not update(Description_1)
		and not update(WhseItem)
		and not update(ServiceItem)
		and not update(ubIIOnLineItem)
		and not update(ItemActive)
		and not update(ubIIReSubmit)
		return;

	-- Logic to insert into InventoryItemsUpdateQueue
	insert into BosmanCommerce7..InventoryItemsUpdateQueue (
					InventoryItemId,
					Status,
					RetryCount,
					DateTimeAdded,
					OptimisticLockField
				)
		select StockLink,
			   0,
			   0,
			   getdate(),
			   0
			from inserted;

	-- Resetting ubIIReSubmit if needed
	if update(ubIIReSubmit)
		and (select ubIIReSubmit from inserted) = 1
		update StkItem
			set ubIIReSubmit = 0
			where ubIIReSubmit = 1
				and StockLink in (select
						StockLink
					from inserted);
end;
go
