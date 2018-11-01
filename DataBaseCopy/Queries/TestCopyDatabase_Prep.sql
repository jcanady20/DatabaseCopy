SET NOCOUNT ON
use db1
Go
Declare @ts INT
SET @ts = 0
while (@ts < 10)
Begin
	SET @ts = @ts + 1
	Declare @tsql Varchar(500)
	SET @tsql = '
		IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N''[dbo].[tbl' + Convert(varchar, @ts) + ']'') AND type in (N''U''))
		DROP TABLE [dbo].[tbl' + Convert(varchar, @ts) + ']
		Create Table [dbo].[tbl' + Convert(varchar, @ts) + ']
		(
			pkey int not null,
			pvalue varchar(20),
			First varchar(30),
			Last varchar(40)
		)
	'
	Execute(@tsql)
End
GO

Use db2
GO
Declare @ts INT
SET @ts = 0
while (@ts < 10)
Begin
	SET @ts = @ts + 1
	Declare @tsql Varchar(500)
	SET @tsql = '
		IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N''[dbo].[tbl' + Convert(varchar, @ts) + ']'') AND type in (N''U''))
		DROP TABLE [dbo].[tbl' + Convert(varchar, @ts) + ']
		Create Table [dbo].[tbl' + Convert(varchar, @ts) + ']
		(
			pkey int not null,
			pvalue varchar(20),
			First varchar(30),
			Last varchar(40)
		)
	'
	Execute(@tsql)
End
GO

use db1
GO
-- Insert Values into the tables

Declare @ts INT
SET @ts = 0
while (@ts < 10)
Begin
	SET @ts = @ts + 1
	Declare @tsql Varchar(8000)
	SET @tsql = '
		SET NOCOUNT ON
		Declare @wt varchar(max)
		Declare @ln INT
		SET @wt = ''In 1905 Albert Einstein published a paper on a special theory of relativity in which he proposed that space and time be combined into a single construct known as spacetime. In this theory the speed of light in a vacuum is the same for all observers which has the result that two events that appear simultaneous to one particular observer will not be simultaneous to another observer if the observers are moving with respect to one another. Moreover an observer will measure a moving clock to tick more slowly than one which is stationary with respect to them; and objects are measured to be shortened in the direction that they are moving with respect to the observer. Over the following ten years Einstein worked on a general theory of relativity, which is a theory of how gravity interacts with spacetime. Instead of viewing gravity as a force field acting in spacetime, Einstein suggested that it modifies the geometric structure of spacetime itself.[19] According to the general theory, time goes more slowly at places with lower gravitational potentials and rays of light bend in the presence of a gravitational field. Scientists have studied the behaviour of binary pulsars, confirming the predictions of Einsteins theories and Non-Euclidean geometry is usually used to describe spacetime.''
		SET @ln = LEN(@wt)

		Declare @i INT
		SET @i = 0
		while(@i < ' + Convert(varchar, @ts * 10000) + ')
		Begin
			SET @i = @i + 1
			
			Insert into [tbl' + Convert(varchar, @ts) + '] (pkey, pvalue, [First], [Last])
			Values(@i, Substring(@wt, Convert(int,Rand() * 100 + Rand() * 100), 20), Substring(@wt, Convert(int,Rand() * 100 + Rand() * 100), 30), Substring(@wt, Convert(int,Rand() * 100 + Rand() * 100), 40))
		End
	'
	Execute(@tsql)
End
--
-- truncate table tbl1
--Declare @wt varchar(8000)
--Declare @ln INT
--SET @wt = 'In 1905 Albert Einstein published a paper on a special theory of relativity in which he proposed that space and time be combined into a single construct known as spacetime. In this theory the speed of light in a vacuum is the same for all observers which has the result that two events that appear simultaneous to one particular observer will not be simultaneous to another observer if the observers are moving with respect to one another. Moreover an observer will measure a moving clock to tick more slowly than one which is stationary with respect to them; and objects are measured to be shortened in the direction that they are moving with respect to the observer. Over the following ten years Einstein worked on a general theory of relativity, which is a theory of how gravity interacts with spacetime. Instead of viewing gravity as a force field acting in spacetime, Einstein suggested that it modifies the geometric structure of spacetime itself.[19] According to the general theory, time goes more slowly at places with lower gravitational potentials and rays of light bend in the presence of a gravitational field. Scientists have studied the behaviour of binary pulsars, confirming the predictions of Einsteins theories and Non-Euclidean geometry is usually used to describe spacetime.'
--SET @ln = LEN(@wt)
--Select Substring(@wt, Convert(int,Rand() * 100), 20 )