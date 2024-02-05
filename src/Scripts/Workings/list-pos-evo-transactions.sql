select top 100 'PostAR', *
	from PostAR
	where 1 = 1
		--and TxDate = '2024-02-05'
		--and AutoIdx > 8
		and ExtOrderNum in ('1511', '1512')
	order by AutoIdx desc

select top 100 'PostGL', *
	from PostGL
	where 1 = 1
		--and TxDate = '2024-02-05'
		--and AutoIdx > 25
		and ExtOrderNum in ('1511', '1512')
		--and Description like '%tip%'
	order by AutoIdx desc

select i.AutoIndex,
	   i.DocType,
	   i.DocState,
	   i.InvNumber,
	   i.OrderDate,
	   i.InvDate,
	   i.DocRepID,
	   i.OrderNum,
	   i.ProjectID,
	   i.ExtOrderNum,
	   i.InvTotExcl,
	   i.InvTotTax,
	   i.InvTotIncl,
	   i.OrdTotExcl,
	   i.OrdTotTax,
	   i.OrdTotIncl,

	   ' | ',

	   l.idInvoiceLines,
	   l.iOrigLineID,

	   l.iWarehouseID,
	   l.iStockCodeID,
	   l.cDescription,

	   l.fQuantity,
	   l.fUnitPriceExcl,
	   l.fUnitPriceIncl,

	   l.fQtyToProcess,
	   l.fQtyLastProcess,
	   l.fQtyProcessed,
	   l.fQtyReserved,

	   l.fQuantityLineTotIncl,
	   l.fQuantityLineTotExcl,
	   l.fQuantityLineTaxAmount,

	   l.fQtyToProcessLineTotIncl,
	   l.fQtyToProcessLineTotExcl,
	   l.fQtyToProcessLineTaxAmount,

	   l.fQtyLastProcessLineTotIncl,
	   l.fQtyLastProcessLineTotExcl,
	   l.fQtyLastProcessLineTaxAmount,

	   l.fQtyProcessedLineTotIncl,
	   l.fQtyProcessedLineTotExcl,
	   l.fQtyProcessedLineTaxAmount,

	   l.bIsWhseItem,
	   l.iTaxTypeID,

	   l.iLineRepID,
	   l.iLineProjectID,
	   l.iLedgerAccountID
	from _btblInvoiceLines l
	  join InvNum i
		  on i.AutoIndex = l.iInvoiceID
	where 1 = 1
		and i.ExtOrderNum in ('1511', '1512')