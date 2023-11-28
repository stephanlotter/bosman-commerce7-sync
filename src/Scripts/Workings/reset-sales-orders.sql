use BosmanCommerce7;

select s.OID,
       s.EmailAddress,
       s.LastErrorMessage,
       s.RetryCount,
       s.RetryAfter,
       s.DatePosted,
       s.EvolutionSalesOrderNumber,
       s.Channel,
       s.CustomerId,
       s.OnlineId,
       s.OrderDate,
       s.OrderNumber,
       s.ShipToAddress1,
       s.ShipToAddress2,
       s.ShipToAddress3,
       s.ShipToAddressCity,
       s.ShipToAddressPostalCode,
       s.OrderValueInVat,
       s.OrderJson,
       s.OptimisticLockField,
       s.GCRecord,
       s.PostingStatus,
       s.ShipToName,
       s.ShipToPhoneNumber,
       s.ShipToAddressProvince,
       s.ShipToAddressCountryCode,
       s.ProjectCode,
       s.CustomerOnlineId
    from OnlineSalesOrder s
    where 1=1
        --and s.LastErrorMessage like 'Inv%'
    order by s.OrderDate desc;
select *
    from OnlineSalesOrderLine
    order by Sku;



/*
update OnlineSalesOrderLine set Sku='BOSNER14' where LineType=1
*/

/*
update OnlineSalesOrder set RetryAfter = getdate(),
      RetryCount = 0,
      PostingStatus = 0,
      LastErrorMessage = null;
*/

/*

delete from OnlineSalesOrderLine
delete from OnlineSalesOrderProcessingLog
delete from OnlineSalesOrder
update ValueStore set KeyValue='2023-11-01'  where KeyName='sales-orders-sync-last-synced'

*/