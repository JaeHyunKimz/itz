        private void 판토스특송부킹(string comcd, string time, MieMgrEventArgs e, ref int prePercent, ref int nowPercent, ref int errorCount, CourierLink courierlink, TB_HDRYC_SERVICE_IRepository hdrycserviceRep, ref string connectionId, string hdryccntrctse, JArray jsonParameters, DataTable hdrycDt, string hdrycCode, ref JObject jobj, JObject token, List<JToken> bzmordernos, ref int queuesn, JArray wbqueueArray, JArray 송장출력오류리스트, JArray tbwaybillArray, JObject 발송자정보, ref int index)
        {
            foreach (var bzmorderno in bzmordernos)
            {
                try
                {
                    connectionId = connections.getConeectionId(comcd, time);
                }
                catch (Exception ex)
                {
                    loggers.Error(string.Format("comcd: [{0}] time: [{1}] connectionId: [{2}] bzmorderno: [{3}] error: [{4}]", comcd, time, connectionId, bzmorderno.GetValueToString(), ex));
                }

                nowPercent = (int)(((float)index / (float)bzmordernos.Count) * 400);
                string 쇼핑몰택배사코드 = string.Empty;

                if (prePercent != nowPercent)
                {
                    e.Msg = string.Empty;
                    e.NowIndex = index + 1;
                    e.TotalCount = bzmordernos.Count;
                    this.CallProgressChanged(progressIndex, e);
                    prePercent = nowPercent;
                }
                index++;
                string orderBoxSize = jsonParameters.AsEnumerable().Where(r => r["paramInput"]["p_bzmorderno"].GetValueToString() == bzmorderno.GetValueToString()).Select(r => r["paramInput"]["p_specificBoxsize"]).FirstOrDefault().GetValueToString();
                DataTable dt = hdrycserviceRep.GetCourierTargetByBZM_ORDERNO(comcd, bzmorderno.ToString());
                if (dt.Rows.Count < 1) //출력가능 주문 없으면 건너뛰기
                    continue;
                if (!전화번호정합성체크(dt.Rows[0]["ADDRSE_TELNO"].ToString()) ||
                    !전화번호정합성체크(dt.Rows[0]["ADDRSE_MOBLNO"].ToString()))
                {
                    errorCount++;

                    try
                    {
                        //화면그리기
                        Clients.Client(connectionId).messageWrite(bzmorderno.ToString(), "<font style='color:red'>수취인 연락처를 확인해주세요. </font>");
                    }
                    catch (Exception ex)
                    {
                        loggers.Error(string.Format("comcd: [{0}] time: [{1}] connectionId: [{2}] bzmorderno: [{3}] error: [{4}]", comcd, time, connectionId, bzmorderno.GetValueToString(), ex));
                    }
                    continue;
                }

                string adrestel1 = 전화번호그룹1추출(dt.Rows[0]["ADDRSE_TELNO"].ToString());
                string adrestel2 = 전화번호그룹2추출(dt.Rows[0]["ADDRSE_TELNO"].ToString());
                string adrestel3 = 전화번호그룹3추출(dt.Rows[0]["ADDRSE_TELNO"].ToString());

                string adresmob1 = 전화번호그룹1추출(dt.Rows[0]["ADDRSE_MOBLNO"].ToString());
                string adresmob2 = 전화번호그룹2추출(dt.Rows[0]["ADDRSE_MOBLNO"].ToString());
                string adresmob3 = 전화번호그룹3추출(dt.Rows[0]["ADDRSE_MOBLNO"].ToString());

                #region 택배사 연동 패킷
                JObject orderInfo = new JObject();
                orderInfo["token"] = token["vo_errmsg"].ToString(); //토큰
                orderInfo["AuthInfo"] = token["AuthInfo"]; //토큰
                orderInfo["CNTRCT_CODE"] = hdrycDt.Rows[0]["CNTRCT_CODE"].ToString(); //계약된 고객사 코드
                orderInfo["CUST_USE_NO"] = dt.Rows[0]["BZM_ORDERNO"].ToString(); // 기업고객이 관리하는 주문번호/영수번호
                orderInfo["RCPT_DV"] = "01"; //접수구분 1: 일반, 02: 반품
                orderInfo["WORK_DV_CD"] = "01"; //작업구분 코드 01: 일반, 02: 교환, 03: A/S 
                orderInfo["REQ_DV_CD"] = "01"; //요청구분코드 01: 요청, 02: 취소
                orderInfo["MPCK_KEY"] = ""; //합포장 키 (Portal 에서 생성) 주문번호 + 운송장번호 -- 추후 로우별 송장관리 경우 생각하여
                orderInfo["CAL_DV_CD"] = "01"; //정산구분코드 01: 계약 운임
                string FRT_DV_CD = "";
                switch (hdryccntrctse)
                {
                    case "A10":
                        FRT_DV_CD = "03";
                        break;
                    case "B10":
                        FRT_DV_CD = "01";
                        break;
                    case "C10":
                        FRT_DV_CD = "02";
                        break;

                }
                orderInfo["FRT_DV_CD"] = FRT_DV_CD; // 운임구분 01: 선불, 02: 착불, 03: 신용
                orderInfo["CNTR_ITEM_CD"] = "01"; //계약 품목 01: 일반 품목
                orderInfo["BOX_TYPE_CD"] = orderBoxSize; //박스타입 01: 극소, 02: 소, 03: 중, 04: 대, 05: 특대 
                orderInfo["BOX_QTY"] = "1"; //박스수량 
                orderInfo["CUST_MGMT_DLCM_CD"] = hdrycDt.Rows[0]["MANAGE_BCNC_CODE"].ToString();//hdrycDt.Rows[0]["CNTRCT_CODE"].ToString(); //고객관리거래처코드
                orderInfo["SENDR_NM"] = 발송자정보["sendernm"].ToString();
                orderInfo["SENDR_TEL_NO1"] = 발송자정보["sendertel1"].GetValueToString();
                orderInfo["SENDR_TEL_NO2"] = 발송자정보["sendertel2"].GetValueToString();
                orderInfo["SENDR_TEL_NO3"] = 발송자정보["sendertel3"].GetValueToString();
                orderInfo["SENDR_CELL_NO1"] = 발송자정보["sendermob1"].GetValueToString();
                orderInfo["SENDR_CELL_NO2"] = 발송자정보["sendermob2"].GetValueToString();
                orderInfo["SENDR_CELL_NO3"] = 발송자정보["sendermob3"].GetValueToString();
                orderInfo["SENDR_ZIP_NO"] = 발송자정보["senderzip"].GetValueToString();
                orderInfo["SENDR_ADDR"] = 발송자정보["senderAdres"].GetValueToString();
                orderInfo["SENDR_DETAIL_ADDR"] = 발송자정보["senderAdresDetail"].GetValueToString();

                JArray Array = new JArray();

                foreach (DataRow dr in dt.Rows)
                {
                    JObject item = new JObject();
                    item["coNo"] = dt.Rows[0]["SOPMAL_DLVY_NO"].ToString(); // 장바구니번호(쇼핑몰 배송번호)
                    item["hblNo"] = ""; // 공란
                    item["boxQty"] = "1".Equals(dr["ROW_NUM"].ToString()) ? "1" : "0";  //동일 장바구니 번호일 경우(합포장) 맨 위 주문에 1 그 밑은 0 
                    item["cneeNm"] = dt.Rows[0]["ADDRSE_NM"].ToString(); // 수취인 성명
                    item["cneeNm1"] = "";

                    item["cneeNm2"] = "";
                    item["cneeAddr1"] = dt.Rows[0]["ADDRSE_ADRES"].ToString() + " " + dt.Rows[0]["ADDRSE_ADRES1"].ToString();  // 수취인 주소
                    item["cneeAddr2"] = "";
                    item["cneeAddr3"] = "".Equals(dt.Rows[0]["CITY"].ToString()) ? "값없음" : dt.Rows[0]["CITY"].ToString(); // 수취친 도시
                    item["cneeAddr4"] = "".Equals(dt.Rows[0]["STATE"].ToString()) ? "" : dt.Rows[0]["STATE"].ToString();

                    item["cneeZipcd"] = dt.Rows[0]["ADDRSE_ZIP"].ToString(); // 수취인 우편 번호
                    item["cneeNatnCd"] = "JP";  //임의값 고정 수정필요                    
                    item["cneeTelNo"] = dt.Rows[0]["ADDRSE_MOBLNO"].ToString().Length > 0 ? dt.Rows[0]["ADDRSE_MOBLNO"].ToString() : dt.Rows[0]["ADDRSE_TELNO"].ToString(); // 수취인 전화번호
                    item["cneeEmailAddr"] = 발송자정보["frwar_email"].GetValueToString(); //발송지 이메일
                    item["polCd"] = 발송자정보["strtpnt_code"].GetValueToString();  //출발코드

                    item["podCd"] = 발송자정보["aloc_code"].GetValueToString();  //도착코드
                    item["itemCd"] = "COSMETIC"; //임의값 고정 수정필요
                    item["itemNm"] = dr["ENG_GOODSNM"].ToString();
                    item["orgnNatnCd"] = "KR";  //임의값 고정 수정필요
                    item["untprc"] = dr["ORDER_SLEPC"].ToString();

                    item["curCd"] = "JPY";  //임의값 고정 수정필요
                    item["itemQty"] = dr["ORDER_QY"].ToString();
                    item["hsCd"] = "";
                    item["homepageAddr"] = dt.Rows[0]["REPRSNT_IMAGE_FILE"].ToString();
                    item["wthLen"] = "";

                    item["vertLen"] = "";
                    item["hgt"] = "";
                    item["wgt"] = dr["WT"].ToString();
                    item["wgtUnitCd"] = "1";
                    item["rmk"] = dt.Rows[0]["DLVY_MEMO"].ToString();

                    item["etcNm1"] = "";
                    item["frgttermCd"] = "";
                    item["carrCd"] = "";
                     
                    쇼핑몰택배사코드 = JObject값(dr["BZM_MEMO"].ToString(), "hdryccode");
                    string 판토스특송현지택배사코드 = string.Empty;
                    if (쇼핑몰택배사코드 == "LX JPPOST")
                    {
                        판토스특송현지택배사코드 = "JPP";
                    }
                    else if (쇼핑몰택배사코드 == "LX PANTOS")
                    {
                        판토스특송현지택배사코드 = string.Empty;
                    }
                    else
                    {
                        판토스특송현지택배사코드 = string.Empty;
                    }

                    item["expsSvcTypeCd"] = 판토스특송현지택배사코드;
                    item["itemId"] = "11";  //임의값 고정 수정필요

                    item["paymComNm"] = "";
                    item["paymId"] = "";
                    item["etcNm12"] = "";
                    item["mblNo"] = "";
                    item["domDlvYn"] = "";

                    item["taxDscrnNo"] = "";
                    item["shppPaymMthdCd"] = "";
                    item["etcNm13"] = "";
                    item["etcNm14"] = "";
                    item["shppNm"] = "";

                    item["shppTelNo"] = "";
                    item["shppAddr"] = "";
                    item["cstTrnExpsAmt"] = "";
                    item["cstTrnExpsCurCd"] = "";
                    item["sndrZipcd"] = "";

                    Array.Add(item);
                }
                orderInfo["ARRAY"] = Array;
                #endregion
                
                try
                {
                    jobj = courierlink.Courierreceipt(orderInfo).Result;
                }
                catch (Exception ex)
                {
                    jobj["vo_code"] = "ERROR";
                    jobj["vo_errmsg"] = ex.Message.ToString();

                    loggers.Error(string.Format("comcd: [{0}] time: [{1}] connectionId: [{2}] bzmorderno: [{3}] error: [{4}]", comcd, time, connectionId, bzmorderno.GetValueToString(), ex));
                }

                //운송장 전송 큐 등록
                if (jobj["vo_code"].GetValueToString() == "OK")
                {
                    string 특송택배사코드 = "A00133"; // 	LX판토스 특송이 기본값

                    if (쇼핑몰택배사코드 == "LX JPPOST")
                    {
                        특송택배사코드 = "A00134";
                    }
                    else if (쇼핑몰택배사코드 == "LX PANTOS")
                    {
                        특송택배사코드 = "A00133";
                    }
                    else
                    {
                        특송택배사코드 = "A99999"; // 값이 매칭되지 않을경우 기타택배사로 등록
                    }

                    var 현지택배사송장번호 = string.Empty;
                    var 특송HBL번호 = string.Empty;
                    var 현지택배사코드 = string.Empty;
                    var 현지택배사코드비젬코드변환 = string.Empty;

                    int 실패카운터 = 0;

                    foreach (var item in jobj["vo_waybill"])
                    {
                        if (item["hblNo"].GetValueToString().Length > 0 && 
                            ("success".Equals(item["result"].GetValueToString()) || (item["result"].ToString() == "fail" && (item["errMsg"].ToString().Contains("Order No Duplicate") || item["errMsg"].ToString().Contains("Duplicate reference order number")))))
                        {
                            현지택배사코드 = item["domCarrCd"].GetValueToString();
                            현지택배사송장번호 = item["domTrnNo"].GetValueToString();
                            특송HBL번호 = item["hblNo"].GetValueToString();
                            switch (현지택배사코드)
                            {                                    
                                case "SGW":
                                    현지택배사코드비젬코드변환 = "A00110";
                                    break;
                                case "JPP":
                                    현지택배사코드비젬코드변환 = "A00099";
                                    break;
                                default:
                                    현지택배사코드비젬코드변환 = "A99999";
                                    break;
                            }
                        }
                        else
                        {
                            if (++실패카운터 == 1)
                            {
                                errorCount++;

                                string errmsg = item["errMsg"].GetValueToString();
                                try
                                {
                                    Clients.Client(connectionId).messageWrite(bzmorderno.GetValueToString(), "<font style='color:red'>" + errmsg + "</font>");
                                }
                                catch (Exception ex)
                                {
                                    loggers.Error(string.Format("comcd: [{0}] time: [{1}] connectionId: [{2}] bzmorderno: [{3}] error: [{4}]", comcd, time, connectionId, bzmorderno.GetValueToString(), ex));
                                }                                
                            }

                            //택배사코드 = item["result"].ToString();
                            //택배송장번호 = item["soNo"].ToString();
                            //특송HBL번호 = item["hblNo"].ToString();
                        }
                    }

                    if (실패카운터 == 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            JObject wbqueue = new JObject();
                            wbqueue["p_bzmorderno"] = dr["BZM_ORDERNO"].ToString();
                            wbqueue["p_bzmordersn"] = dr["BZM_ORDER_SN"].ToString();
                            wbqueue["p_hdryccode"] = 특송택배사코드;
                            wbqueue["p_waybilno"] = 특송HBL번호;
                            wbqueue["p_tbtrnswbqueuesn"] = queuesn++;
                            wbqueue["p_hdryccode2"] = 현지택배사코드비젬코드변환;
                            wbqueue["p_waybilno2"] = 현지택배사송장번호;
                            wbqueueArray.Add(wbqueue);
                        }

                        JObject tbwaybillInfo = new JObject();
                        tbwaybillInfo["p_bzmorderno"] = dt.Rows[0]["BZM_ORDERNO"].ToString();
                        tbwaybillInfo["p_hdryccode"] = 특송택배사코드;
                        tbwaybillInfo["p_waybilno"] = 특송HBL번호;
                        tbwaybillInfo["p_hdryccode2"] = 현지택배사코드비젬코드변환;
                        tbwaybillInfo["p_waybilno2"] = 현지택배사송장번호;
                        tbwaybillArray.Add(tbwaybillInfo);
                    }

                }
                else
                {
                    errorCount++;

                    string errmsg = jobj["vo_errmsg"].GetValueToString();
                    
                    try
                    {
                        Clients.Client(connectionId).messageWrite(bzmorderno.GetValueToString(), "<font style='color:red'>" + errmsg + "</font>");
                    }
                    catch (Exception ex)
                    {
                        loggers.Error(string.Format("comcd: [{0}] time: [{1}] connectionId: [{2}] bzmorderno: [{3}] error: [{4}]", comcd, time, connectionId, bzmorderno.GetValueToString(), ex));
                    }
                    //JObject 송장출력오류정보 = new JObject();
                    //foreach (DataRow dr in dt.Rows)
                    //{
                    //    if (송장출력오류리스트.AsEnumerable().Where(r => r["p_bzmorderno"].ToString() == dr["BZM_ORDERNO"].ToString()).Count() < 1)
                    //    {
                    //        if (jobj["vo_code"].ToString() != "OK") // 택배사 파업으로 인한 오류 제외 
                    //        {
                    //            송장출력오류정보["p_bzmorderno"] = dr["BZM_ORDERNO"].ToString();
                    //            송장출력오류리스트.Add(송장출력오류정보);
                    //        }
                    //    }
                    //}
                }
            }
        }

        private static string JObject값(string 원문, string 속성)
        {
            string 결과 = string.Empty;
            try
            {
                JObject bizmemo = JsonConvert.DeserializeObject<JObject>(원문);
                결과 = bizmemo[속성].GetValueToString();
            }
            catch (Exception)
            {
            }
            return 결과;
        }

        private static string JObject값(JObject 원문, string 속성)
        {
            string 결과 = string.Empty;
            try
            {
                결과 = 원문[속성].GetValueToString();
            }
            catch (Exception)
            {
            }
            return 결과;
        }