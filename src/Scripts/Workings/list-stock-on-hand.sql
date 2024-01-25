use BosmanBFV

declare @sku Nvarchar(20) = 'BOSCAS14U',
		@warehouseCode Nvarchar(20) = '026',
		@InventoryItemId Int,
		@WarehouseId Int;

select @InventoryItemId = StockLink
	from StkItem
	where cSimpleCode = @sku;

--select StockLink,
--	   Code,
--	   cSimpleCode,
--	   Description_1,
--	   ServiceItem,
--	   ItemActive
--	from StkItem
--	where StockLink = 976;


select @WarehouseId = wm.WhseLink
	from WhseMst wm
	where wm.Code = @warehouseCode;

select si.cSimpleCode Sku,
	   wm.Code WarehouseCode,
	   ws.WHWhseID,
	   ws.WHStockLink,
	   ws.WHQtyOnHand QuantityOnHand,
	   ws.WHQtyOnSO QuantityOnSalesOrder,
	   ws.WHQtyReserved QuantityReserved
	from WhseStk ws
	  join StkItem si
		  on si.StockLink = ws.WHStockLink
	  join WhseMst wm
		  on wm.WhseLink = ws.WHWhseID
	where 1 = 1
		and ws.WHQtyOnHand > 0
		and ws.WHWhseID = @WarehouseId
		and ws.WHStockLink = @InventoryItemId;


