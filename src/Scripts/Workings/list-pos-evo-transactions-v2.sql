--use BAWINE
--use BAWINE_UAT
use BosmanBFV

declare @evoOrderNumber nvarchar(20) = 'BSO037617',
		@c7OrderNumber nvarchar(20) = '2036',
		@c7RefundOrderNumber nvarchar(20) = '2037'
		;

select top 100 DocType, 
		DocState,
		ExtOrderNum,
		OrderNum,
		InvNumber,
		InvDate,
		OrderDate,
		OrdTotIncl, 
		AutoIndex
	from InvNum 
	where 1=1
		--and AutoIndex > 91016
		and ExtOrderNum in (@c7OrderNumber, @c7RefundOrderNumber)
		--and ExtOrderNum = @c7OrderNumber
	order by AutoIndex desc

select top 100 a.Account, 
	p.Id,
	p.Debit,
	p.Credit,
	p.Description,
	p.Reference,
	p.cReference2,
	p.ExtOrderNum,
		p.* 
	from PostGL p
		join Accounts a
			on a.AccountLink = p.AccountLink
	where 1=1
		--and p.AutoIdx > 1152483
		--and p.TxDate >= '2024-06-04'
		--and p.Order_No = @evoOrderNumber
		and p.ExtOrderNum in (@c7OrderNumber, @c7RefundOrderNumber)
		--and p.ExtOrderNum = @c7OrderNumber
		--and p.TrCodeID = 101
		--and p.Description like '%pay%'
	order by p.AutoIdx desc


select top 100 a.Account ,p.* 
	from PostAR p
		join Client a
			on a.DCLink = p.AccountLink
	where 1=1
		--and p.AutoIdx > 439358
		and p.TxDate >= '2024-04-25'
		--and p.Order_No = @evoOrderNumber
		and p.ExtOrderNum in (@c7OrderNumber, @c7RefundOrderNumber)
		--and p.ExtOrderNum = @c7OrderNumber
		--and p.TrCodeID = 101
	order by p.AutoIdx desc


select top 100 a.Code ,p.* 
	from PostST p
		join StkItem a
			on a.StockLink = p.AccountLink
	where 1=1
		--and p.AutoIdx > 439358
		and p.TxDate >= '2024-04-25'
		--and p.Order_No = @evoOrderNumber
		and p.ExtOrderNum in (@c7OrderNumber, @c7RefundOrderNumber)
		--and p.ExtOrderNum = @c7OrderNumber
		--and p.TrCodeID = 101
	order by p.AutoIdx desc

