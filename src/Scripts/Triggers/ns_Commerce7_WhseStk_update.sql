use BosmanBFV;

-- Check if the trigger 'ns_Commerce7_WhseStk_update' already exists and drop it if it does
drop trigger if exists ns_Commerce7_WhseStk_update;
go

-- Create the trigger 'ns_Commerce7_WhseStk_update' on dbo.Client table
create trigger dbo.ns_Commerce7_WhseStk_update
on [dbo].[WhseStk]
after insert, update
as
begin
	-- Change History
	-- 2023-11-25 08:57: Trigger Created

	set nocount on;
	if TRIGGER_NESTLEVEL(@@procid) > 1
		return;

	-- Exit if it's not the POS warehouse being updated
	if not exists (select
				1
			from inserted i
			join WhseMst wm
				on i.WHWhseID = wm.WhseLink
				and wm.Code in ('035', '002'))
		return;

	insert into BosmanCommerce7.dbo.StockLevelUpdateQueue (
					WarehouseId,
					ItemId,
					Status,
					RetryCount,
					DateTimeAdded,
					OptimisticLockField
				)
		select WHWhseID,
			   WHStockLink,
			   0,
			   0,
			   getdate(),
			   0
			from inserted;

end;
go