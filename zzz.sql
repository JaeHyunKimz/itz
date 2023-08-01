
    <insert id="insertCancelOT"  parameterType="hashmap"  >
    ---컬럼 재수정 해야함.
        INSERT INTO ASLIB.EMALL_CANCEL_DTL
             (
              BRAND_ID
             ,CSTMR_ID
             ,CSTMR_SNAME
             ,ORDER_NO
            ,ORIGIN_ORDER_NO
             
            ,PRODUCT_SKU
            ,PRODUCT_CODE 
            ,PRODUCT_COLOR
            ,PRODUCT_SIZE
            ,PRODUCT_QTY
           
            ,PRODUCT_UNIT_PRICE
            ,PRODUCT_AMT
            ,EMONEY
            ,COUPON_SALE
            ,DELIVERY_SALE_AMT
            
            ,DELIVERY_CHARGE
            ,CANCEL_DATE
            ,CANCEL_TIME
            ,ADD_DATE
            ,FILE_NAME
             
            , REMARK1  
            , REMARK2  
            ,RETURN_TYPE
             )
        VALUES (
            '2',
             'BB990',
             'OKR 자사몰',
             #{order_no,jdbcType=VARCHAR},
             #{origin_order_no,jdbcType=VARCHAR},
             
             #{product_sku,jdbcType=VARCHAR},
             SUBSTR(#{product_sku,jdbcType=VARCHAR},1,9),
             CASE LENGTH(#{product_sku,jdbcType=VARCHAR})  
                   WHEN 16 THEN SUBSTR(#{product_sku,jdbcType=VARCHAR},10,4) 
                   WHEN 15 THEN SUBSTR(#{product_sku,jdbcType=VARCHAR},10,3) 
                   WHEN 14 THEN SUBSTR(#{product_sku,jdbcType=VARCHAR},10,2) 
             END ,
             CASE LENGTH(#{product_sku,jdbcType=VARCHAR})  
                   WHEN 16 THEN SUBSTR(#{product_sku,jdbcType=VARCHAR},14,3) 
                   WHEN 15 THEN SUBSTR(#{product_sku,jdbcType=VARCHAR},13,3) 
                   WHEN 14 THEN SUBSTR(#{product_sku,jdbcType=VARCHAR},12,3) 
             END ,
             (TO_NUMBER(DECODE(#{product_qty,jdbcType=INTEGER},'',0,#{product_qty,jdbcType=INTEGER}))*TO_NUMBER(#{cal}) ),
             
              DECODE(#{product_unit_price,jdbcType=INTEGER},'',0,#{product_unit_price,jdbcType=INTEGER}),
             (TO_NUMBER(DECODE(#{product_amt,jdbcType=INTEGER},'',0,#{product_amt,jdbcType=INTEGER}))*TO_NUMBER(#{cal})),
             (TO_NUMBER(DECODE(#{emoney,jdbcType=INTEGER},'',0,#{emoney,jdbcType=INTEGER}))*TO_NUMBER(#{cal})),
             (TO_NUMBER(DECODE(#{coupon_sale,jdbcType=INTEGER},'',0,#{coupon_sale,jdbcType=INTEGER}))*TO_NUMBER(#{cal})),
             DECODE(#{delivery_sale_amt,jdbcType=INTEGER},'',0,#{delivery_sale_amt,jdbcType=INTEGER}),
             
             DECODE(#{delivery_charge,jdbcType=INTEGER},'',0,#{delivery_charge,jdbcType=INTEGER}),
             TO_CHAR(to_date(#{order_cancel_date,jdbcType=VARCHAR},'YYYY-MM-DD HH24:MI:SS'),'YYYYMMDD'),
             TO_CHAR(to_date(#{order_cancel_date,jdbcType=VARCHAR},'YYYY-MM-DD HH24:MI:SS'),'HH24:MI:SS'),
             TO_CHAR(SYSDATE,'YYYYMMDD'),
             #{file_name,jdbcType=VARCHAR},
             
             #{remark1,jdbcType=VARCHAR},
             #{remark2,jdbcType=VARCHAR},
             #{return_type,jdbcType=VARCHAR}
            

          )
  </insert>
