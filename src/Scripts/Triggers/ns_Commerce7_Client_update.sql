use BosmanBFV;

-- Check if the trigger 'ns_Commerce7_Client_update' already exists and drop it if it does
drop trigger if exists ns_Commerce7_Client_update;
go

-- Create the trigger 'ns_Commerce7_Client_update' on dbo.Client table
create trigger dbo.ns_Commerce7_Client_update
on dbo.Client
after insert, update
as
begin
	-- Change History
	-- 2023-11-25 08:57: Trigger Created

	set nocount on;
	if TRIGGER_NESTLEVEL(@@procid) > 1
		return;

	-- Exit trigger if DCBalance column was updated
	if update(DCBalance)
		return;

	if not update(ucARbIsPosCustomer)
		and (select isnull(ucARbIsPosCustomer, 0) from inserted) = 0
		return;

	if not update(ON_HOLD)
		and (select isnull(ON_HOLD, 0) from inserted) = 1
		return;

	-- Define columns to check for update
	if update(ucARbboxaccountno)
		or update(On_Hold)
		or update(name)
		or update(contact_person)
		or update(email)
		or update(ucARwcEmail)

		or update(Physical1)
		or update(Physical2)
		or update(Physical3)
		or update(Physical4)
		or update(Physical5)
		or update(PhysicalPC)

		or update(Post1)
		or update(Post2)
		or update(Post3)
		or update(Post4)
		or update(Post5)
		or update(PostPC)

		or update(account)
	begin

		insert into BosmanCommerce7.dbo.CustomerUpdateQueue (
						CustomerId
					)
			select DCLink
				from inserted;

	end;
end;
go