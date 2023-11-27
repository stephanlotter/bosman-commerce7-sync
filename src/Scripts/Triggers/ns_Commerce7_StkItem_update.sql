-- Drops the trigger if it already exists
drop trigger if exists ns_Commerce7_StkItem_update;
go

-- Trigger definition
create trigger ns_Commerce7_StkItem_update
on StkItem
after insert, update
as
begin
	-- Disable the count message
	set nocount on;
	-- Prevent nested trigger firing
	if TRIGGER_NESTLEVEL(@@procid) > 1
		return;

	-- Exit condition 1: Checking ubIIOnLineItem column
	if not update(ubIIOnLineItem)
		and (select ubIIOnLineItem from inserted) = 0
		return;
	-- Exit condition 2: Checking ItemActive column
	if not update(ItemActive)
		and (select ItemActive from inserted) = 0
		return;

	-- Exit condition 3: Checking quantity related columns
	if update(Qty_On_Hand)
		or update(QtyOnPO)
		or update(QtyOnSO)
		or update(ReservedQty)
		return;

	-- Exit condition 4: Check for any updated monitor columns
	if not update(ubIIOnLineItem)
		and not update(ItemActive)
		and not update(Qty_On_Hand)
		and not update(QtyOnPO)
		and not update(QtyOnSO)
		and not update(ReservedQty)
	begin
		return;
	end

	-- Logic to insert into StockItemUpdateQueue
	insert into StockItemUpdateQueue (
					StockLink
				)
		select StockLink
			from inserted;

	-- Resetting ubIIReSubmit if needed
	update StkItem
		set ubIIReSubmit = 0
		where ubIIReSubmit = 1
			and StockLink in (select
					StockLink
				from inserted);
end;
go
