use BosmanCommerce7;

select * from OnlineSalesOrder s order by s.OrderDate desc;
select * from OnlineSalesOrderLine order by OnlineSalesOrder;

/*
update OnlineSalesOrder set RetryAfter = getdate(),
      RetryCount = 0,
      PostingStatus = 0,
      LastErrorMessage = null;
*/

/*
delete from OnlineSalesOrderLine
delete from OnlineSalesOrder
update ValueStore set KeyValue='2023-08-01'  where KeyName='sales-orders-sync-last-synced'
*/