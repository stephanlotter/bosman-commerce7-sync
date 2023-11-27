-- Check if the trigger 'ns_Commerce7_Client_update' already exists and drop it if it does
if object_id('dbo.ns_Commerce7_Client_update', 'TR') is not null
	drop trigger dbo.ns_Commerce7_Client_update;
go

-- Create the trigger 'ns_Commerce7_Client_update' on dbo.Client table
create trigger dbo.ns_Commerce7_Client_update
on dbo.Client
after insert, update
as
begin
	-- Prevent trigger re-entry
	if exists (select
				1
			from sys.triggers
			where parent_id = object_id('dbo.Client')
			and object_id = @@nestlevel)
		return;

	-- Exit trigger if DCBalance column was updated
	if update(DCBalance)
		return;

	-- Exit if ucARbboxaccountno field is '0' before and after the update
	if (select isnull(ucARbboxaccountno, 0) from inserted) = '0'
		and (select isnull(ucARbboxaccountno, 0) from deleted) = '0'
		return;

	-- Exit if ON_HOLD field is '1' 
	if (select isnull(ON_HOLD, 0) from inserted) = '1'
		return;

	-- Define columns to check for update
	declare @updated Int = 0;
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

		set @updated = 1;

	-- Exit if none of the specified columns were updated
	if @updated = 0
		return;


	-- Insert into CustomerUpdateQueue table in the BosmanCommerce7 database
	insert into BosmanCommerce7.dbo.CustomerUpdateQueue (
					CustomerId
				)
		select DCLink
			from inserted;
end;
go