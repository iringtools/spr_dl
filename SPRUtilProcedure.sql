/****** Object:  StoredProcedure [dbo].[UPDATE_LabelNames]    Script Date: 07/29/2013 14:29:54 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UPDATE_LabelNames]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[UPDATE_LabelNames]
GO

-- Drop UPDATE_LabelNames before tblTypeLabelNames as its referenced there.
/****** Object:  UserDefinedTableType [dbo].[tblTypeLabelNames]    Script Date: 07/29/2013 14:49:10 ******/
IF  EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'tblTypeLabelNames' AND ss.name = N'dbo')
DROP TYPE [dbo].[tblTypeLabelNames]
GO

/****** Object:  UserDefinedTableType [dbo].[tblTypeLabelNames]    Script Date: 07/29/2013 14:49:10 ******/
CREATE TYPE [dbo].[tblTypeLabelNames] AS TABLE(
	[labelNameIndexes] [int] NULL
)
GO



/****** Object:  StoredProcedure [dbo].[UPDATE_LabelNames]    Script Date: 07/29/2013 14:29:54 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



--CREATE TYPE tblTypeLabelNames AS TABLE  
--( labelNameIndexes int)

-- This procedure will insert the blank values on new properties for all the linkages of the SPOOL.

CREATE  PROCEDURE [dbo].[UPDATE_LabelNames](
@SpoolIndex INT,
@parentCursor nvarchar(max),
@guid nvarchar(50),
@tblLabelIndexes  dbo.tblTypeLabelNames READONLY
)

AS
BEGIN
 declare @nextIndex INT
 declare @nextlabellineNo INT
 declare @LabelLineNo INT
 declare @nextLabelIndex INT
 declare @valueIndex INT
 declare @maxIndex INT = 0
 declare @rowCount INT
 declare @q1 nvarchar(Max)
 declare @q2 nvarchar(Max)
 declare @q3 nvarchar(Max)
 declare @q4 nvarchar(Max)
 declare @q5 nvarchar(Max)
 declare @q6 nvarchar(Max)
 declare @q7 nvarchar(Max)
 declare @q8 nvarchar(Max)
 declare @q9 nvarchar(Max)
 
 -- Create a Temp Table for holding new records and manipulations.
    set @q8 = 'IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE=''BASE TABLE'' AND TABLE_NAME= ''Temp_'+@guid +''')'
              +'begin drop table  Temp_'+@guid + ' end'
    exec  sp_executesql @q8
	
    set @q1 = 'select * into Temp_' +@guid +'  from labels_'+@guid+' where 1=0'
	exec  sp_executesql @q1
 	set @q9 = 'alter table Temp_' +@guid +'  add primary key (linkage_index,label_name_index)'
	exec  sp_executesql @q9
	
 -- First get the index of blank value, that need to be inserted for all new properties in linkages.  
 	  set @q2 = 'select  @valueIndex = label_value_index from label_values_'+@guid+' where label_value = '''''
      exec sp_executesql @q2 , N'@valueIndex int OUTPUT', @valueIndex OUTPUT
	
  if(@valueIndex is NULL)
  begin
         set @q3 = 'select  @maxIndex = ISNULL(MAX(label_value_index),0)+1 FROM  label_values_'+ @guid
         exec sp_executesql @q3 , N'@maxIndex int OUTPUT', @maxIndex OUTPUT
                     
		 set @q4 ='insert into label_values_'+@guid+' (label_value_index,label_value, label_value_numeric) values 
                         ('+CONVERT(NVARCHAR(50),@maxIndex)+','''',0)';
		 exec  sp_executesql @q4
		 set @valueIndex = @maxIndex
  end
 
    --Child Cursor - Loop through all the new properties of the spool.
        DECLARE Label_Index CURSOR          
                            FOR  select labelNameIndexes from @tblLabelIndexes
    
    --Parent Cursor - Loop through all the linkages of the spool.                         
 --DECLARE Linkage_Index CURSOR               
 --      FOR (select linkage_index,(select ISNULL(Max(label_line_number),0)+1 from labels where linkage_index= A.linkage_index)
 --          as labelNo from labels A where label_name_index = @SpoolIndex )order by linkage_index            
    exec  sp_executesql @parentCursor

    OPEN Linkage_Index  
    FETCH NEXT FROM Linkage_Index INTO @nextIndex ,@nextlabellineNo

    WHILE @@FETCH_STATUS = 0  
    BEGIN  
   
     SET @LabelLineNo = @nextlabellineNo;
             -- Child Cursor Started... 
             -- Insert the blank value for all new properties.
             OPEN Label_Index  
             FETCH NEXT FROM Label_Index INTO @nextLabelIndex 

             WHILE @@FETCH_STATUS = 0  
             BEGIN  
             -- First check if the label is already there or not on that linkage; if not apply it.
 
             set @q7 = 'select  @rowCount = COUNT(*) from labels_'+@guid + ' where linkage_index = ''' + CONVERT(NVARCHAR(50),
			            @nextIndex) + ''' and label_name_index = '''+ CONVERT(NVARCHAR(50),@nextLabelIndex) +''''
             exec sp_executesql @q7 , N'@rowCount INT OUTPUT', @rowCount OUTPUT
	  
			 
             if(@rowCount = 0)
               BEGIN
                  set @q5 = 'Insert into Temp_'+@guid+' (linkage_index,label_name_index,label_value_index,label_line_number,extended_label) values  
                   ('+CONVERT(NVARCHAR(50),@nextIndex)+','+CONVERT(NVARCHAR(50),@nextLabelIndex)+','+CONVERT(NVARCHAR(50),
				     @valueIndex)+','+CONVERT(NVARCHAR(50),@LabelLineNo)+',0)'
                  exec  sp_executesql @q5
                  set @LabelLineNo = @LabelLineNo + 1;       
               End   
             Else
               BEGIN
                set @q6 = 'insert into Temp_'+@guid+' ([linkage_index],[label_name_index],[label_value_index],[label_line_number],[extended_label])
                select [linkage_index],[label_name_index],[label_value_index],[label_line_number],[extended_label]
                from labels_'+@guid+' where linkage_index = '+CONVERT(NVARCHAR(50),@nextIndex)+' and label_name_index = '+
				CONVERT(NVARCHAR(50),@nextLabelIndex)+''
				exec  sp_executesql @q6
               END  
               
             FETCH NEXT FROM Label_Index INTO @nextLabelIndex 
             End  
             CLOSE Label_Index;  
       
    FETCH NEXT FROM Linkage_Index  
    INTO @nextIndex ,@nextlabellineNo
    End  
    
  CLOSE Linkage_Index;  
  DEALLOCATE Label_Index;
  DEALLOCATE Linkage_Index;  
END

GO


/****** Object:  StoredProcedure [dbo].[UPDATE_LabelValues]    Script Date: 07/29/2013 14:30:08 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UPDATE_LabelValues]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[UPDATE_LabelValues]
GO

IF  EXISTS (SELECT * FROM sys.types st JOIN sys.schemas ss ON st.schema_id = ss.schema_id WHERE st.name = N'typeLabelNameAndIndex' AND ss.name = N'dbo')
DROP TYPE [dbo].[typeLabelNameAndIndex]
GO

/****** Object:  UserDefinedTableType [dbo].[typeLabelNameAndIndex]    Script Date: 07/29/2013 14:49:10 ******/
CREATE TYPE typeLabelNameAndIndex AS TABLE  
( labelIndex int,
  lblName nvarchar(100))
GO

/****** Object:  StoredProcedure [dbo].[UPDATE_LabelValues]    Script Date: 07/29/2013 14:30:08 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- This procedure will update the value of the given label or property related 
-- to a specified tag.
CREATE PROCEDURE [dbo].[UPDATE_LabelValues](
@LabelIndexAndName  dbo.typeLabelNameAndIndex READONLY,
@TableName nvarchar(100),
@keyColumnName nvarchar(100),
@guid nvarchar(50)
)
AS
BEGIN
declare @labelNameIndex INT
declare @labelName nvarchar(100)
declare @nextTag nvarchar(100)
declare @nextIndex INT
declare @BlankValueIndex INT
declare @labelValueIndex INT
declare @labelValue nvarchar(100)
declare @labelValueCount INT		
declare @LabelValueRowIndex INT
declare @q1 nvarchar(Max)
declare @q2 nvarchar(Max)
declare @q3 nvarchar(Max)
declare @q4 nvarchar(Max)
declare @q5 nvarchar(Max)
declare @q6 nvarchar(Max)
declare @q7 nvarchar(Max)
declare @innerCursor nvarchar(Max)

-- Keep the blank value index.
set @q1 = 'select  @BlankValueIndex = label_value_index from label_values_'+@guid+' where label_value = '''''
exec sp_executesql @q1 , N'@BlankValueIndex int OUTPUT', @BlankValueIndex OUTPUT

begin transaction
  -- Loop through all the new Labels. 
  DECLARE OuterCursor CURSOR FOR  select labelIndex,lblName from @LabelIndexAndName        
                            
  OPEN OuterCursor  
  FETCH NEXT FROM OuterCursor INTO @labelNameIndex, @labelName
  WHILE @@FETCH_STATUS = 0  
  BEGIN 
	
	-- InnerCursor: Loop through all the tags and the selected label of the proxy table.  
    set @innerCursor = 'DECLARE Proxy_Tags CURSOR FOR select '+ @keyColumnName+ ',' + @labelName + ' from ['+ @TableName + ']'
	exec  sp_executesql @innerCursor
	    
	OPEN Proxy_Tags  
    FETCH NEXT FROM Proxy_Tags INTO @nextTag, @labelValue
    WHILE @@FETCH_STATUS = 0  
    BEGIN 
      --START: Get the Label_Value_index of the label value for the selected tag.
	  -- First check from proxy whether the label has value or not.
	    set @labelValueIndex = ''
		if(@labelValue is null or @labelValue = '')
		   Set @labelValueIndex = @BlankValueIndex  -- No value exist, update with null value .		 
		else    
	       begin 
			  -- Get the index of the value for that label of the corresponding tag.
			  set @q2 = 'select  @labelValueIndex = label_value_index from label_values_'+@guid+' 
					   where label_value = '''+@labelValue+''''
              exec sp_executesql @q2 , N'@labelValueIndex int OUTPUT', @labelValueIndex OUTPUT
	          
			  -- If no label value index found, insert the value and get the index.
              if(@labelValueIndex is null or @labelValueIndex = '')
              begin
				  set @q3 = 'select  @LabelValueRowIndex = ISNULL(MAX(label_value_index),0)+1 FROM  label_values_'+ @guid
                  exec sp_executesql @q3 , N'@LabelValueRowIndex int OUTPUT', @LabelValueRowIndex OUTPUT
	                  
				  set @q4 = 'insert into label_values_'+@guid+' (label_value_index,label_value, label_value_numeric)
                             values ('+CONVERT(NVARCHAR(50),@LabelValueRowIndex)+','''+
				             @labelValue + ''',0)'
				  exec  sp_executesql @q4
				  set @labelValueIndex = @LabelValueRowIndex
			    end					
			end
	  --END: Get the Label_Value_index of the label value for the selected tag.  

	  --Update Values for all the linkages of the specified tag.
        set @q5 = 'update Temp_'+@guid+' set label_value_index = '+CONVERT(NVARCHAR(50),@labelValueIndex)+'  where label_name_index = '+CONVERT(NVARCHAR(50),@labelNameIndex)+'
                   and linkage_index in (select linkage_index from labels_'+@guid+' join label_values_'+@guid+' on labels_'+@guid+'.label_value_index = 
                   label_values_'+@guid+'.label_value_index where label_values_'+@guid+'.label_value = '''+ CONVERT(NVARCHAR(50),@nextTag)+''' )'	 
		exec sp_executesql @q5


	FETCH NEXT FROM Proxy_Tags INTO @nextTag, @labelValue    
    end

    --Finally move data from Temp table to label table.        
      set @q6 = 'delete FROM labels_'+@guid+' where label_name_index = '+CONVERT(NVARCHAR(50),@labelNameIndex) +'
              and linkage_index in (select linkage_index from Temp_'+@guid+')'
	  exec sp_executesql @q6		
      
      set @q7 = 'insert into labels_'+@guid+' ([linkage_index],[label_name_index],[label_value_index],[label_line_number],[extended_label])
      select [linkage_index],[label_name_index],[label_value_index],[label_line_number],[extended_label] from Temp_'+@guid+' where label_name_index = '+CONVERT(NVARCHAR(50),@labelNameIndex)
	  exec sp_executesql @q7

	CLOSE Proxy_Tags; 
    DEALLOCATE Proxy_Tags; 
	
  FETCH NEXT FROM OuterCursor INTO @labelNameIndex, @labelName
  end
	  
commit transaction
	
CLOSE OuterCursor; 
DEALLOCATE OuterCursor; 
END

GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DROP_CacheTables]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[DROP_CacheTables]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE  PROCEDURE [dbo].[DROP_CacheTables](
@guid nvarchar(50)
)

AS
BEGIN
 declare @q1 nvarchar(1000)
 declare @q2 nvarchar(1000)
 declare @q3 nvarchar(1000)
 declare @q4 nvarchar(1000)
 
 set @q1 = 'if exists (select * from INFORMATION_SCHEMA.TABLES where TABLE_NAME = ''Temp_'+@guid + ''')
            drop table Temp_'+ @guid
 exec sp_executesql @q1
 
 set @q2 = 'if exists (select * from INFORMATION_SCHEMA.TABLES where TABLE_NAME = ''label_names_'+@guid + ''')
            drop table label_names_'+ @guid
            
 exec sp_executesql @q2
 
 set @q3 = 'if exists (select * from INFORMATION_SCHEMA.TABLES where TABLE_NAME = ''label_values_'+@guid + ''')
            drop table label_values_'+ @guid
 exec sp_executesql @q3
 
 set @q4 = 'if exists (select * from INFORMATION_SCHEMA.TABLES where TABLE_NAME = ''labels_'+@guid + ''')
            drop table labels_'+ @guid
 exec sp_executesql @q4
	  
End
Go
