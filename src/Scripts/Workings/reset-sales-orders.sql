use BosmanCommerce7;

select * from OnlineSalesOrder s order by s.OrderDate desc;
select * from OnlineSalesOrderLine order by OnlineSalesOrder;

/*
delete from SalesOrderLine
delete from SalesOrder
update ValueStore set KeyValue='2023-08-01'  where KeyName='sales-orders-sync-last-synced'
*/