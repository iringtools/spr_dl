--This is the backup done before making the changes related to valve, hanger etc etc as 
--the changes requested by darius on mail 25June 2013.

USE [SPR]
GO
/****** Object:  StoredProcedure [dbo].[UPDATE_LabelNames]    Script Date: 06/26/2013 13:54:10 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


--CREATE TYPE tblTypeLabelNames AS TABLE  
--( labelNameIndexes int)

-- This procedure will insert the blank values on new properties for all the linkages of the SPOOL.

ALTER  PROCEDURE [dbo].[UPDATE_LabelNames](
@SpoolIndex INT,
@tblLabelIndexes  dbo.tblTypeLabelNames READONLY
)

AS
BEGIN
 declare @nextIndex INT
 declare @nextlabellineNo INT
 declare @LabelLineNo INT
 declare @nextLabelIndex INT
 declare @valueIndex INT
 
 -- First get the index of blank value, that need to be inserted for all new properties in linkages.
 set @valueIndex = (select label_value_index from label_values where label_value = '')
 if(@valueIndex is NULL)
 begin
        set @valueIndex = (SELECT ISNULL(MAX(label_value_index),0)+1 FROM label_values)
        insert into label_values (label_value_index,label_value, label_value_numeric) values 
                         (@valueIndex,'',0);
 end
 
    --Child Cursor - Loop through all the new properties of the spool.
        DECLARE Label_Index CURSOR          
                            FOR  select labelNameIndexes from @tblLabelIndexes
    
    --Parent Cursor - Loop through all the linkages of the spool.                         
 DECLARE Linkage_Index CURSOR               
       FOR (select linkage_index,(select ISNULL(Max(label_line_number),0)+1 from labels where linkage_index= A.linkage_index)
           as labelNo from labels A where label_name_index = @SpoolIndex )order by linkage_index  

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
                  Insert into labels (linkage_index,label_name_index,label_value_index,label_line_number,extended_label) values  
                   (@nextIndex,@nextLabelIndex,@valueIndex,@LabelLineNo,0)
                   
                  set @LabelLineNo = @LabelLineNo + 1;         
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
