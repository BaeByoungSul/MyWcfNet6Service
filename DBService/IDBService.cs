using CoreWCF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace BBS.WCF
{
    /// <summary>
    /// SetTransOption :  ExecNonQuery에서 사용할 TransactionScopeOption
    /// ExecNonQuery(MyCommand[] myCmds) : 서버에서 Transaction 관리
    /// ExecNonQueryWs(MyCommand myCmd): Client에서 Transaction 관리 , .NET 6 지원하지 않아서 삭제
    /// GetDataSetXml(MyCommand myCmd): DB 조회
    /// 2022.06.16, reliableSession이 되지 않아서 InstanceContextMode.PerSession 가 되지 않네요 
    /// </summary>

    [ServiceContract(Namespace = "http://nakdong.wcf.service")]
    public interface IDBService
    {
        [OperationContract]
        SvcReturn ExecNonQuery(MyCommand[] myCmds);

        [OperationContract]
        SvcReturn GetDataSetXml(MyCommand myCmd);
        
      }



    [DataContract]
    public class MyCommand
    {
        public MyCommand()
        {
            CommandName = string.Empty;
            ConnectionName = string.Empty;
            CommandText = string.Empty;
        }

        [DataMember(Order = 0, IsRequired = true)]
        public string CommandName { get; set; }
        [DataMember(Order = 1, IsRequired = true)]
        public string ConnectionName { get; set; }
        [DataMember(Order = 2, IsRequired = true)]
        public int CommandType { get; set; }
        [DataMember(Order = 3, IsRequired = true)]
        public string CommandText { get; set; }

        [DataMember(Order = 4)]
        public MyPara[]? Parameters { get; set; }

        [DataMember(Order = 5)]
        public MyParaValue[][]? ParaValues { get; set; }
    }

    [DataContract]
    public class MyPara
    {
        public MyPara()
        {
            ParameterName = String.Empty;
            HeaderCommandName = String.Empty;
            HeaderParameter = String.Empty;
        }
        [DataMember(Order = 0)]
        public string ParameterName { get; set; }
        [DataMember(Order = 1)]
        public int DbDataType { get; set; }
        [DataMember(Order = 2)]
        public int Direction { get; set; }
        [DataMember(Order = 3)]
        public string HeaderCommandName { get; set; }
        [DataMember(Order = 4)]
        public string HeaderParameter { get; set; }
    }

    [DataContract]
    public class MyParaValue
    {
        public MyParaValue()
        {
            ParameterName = String.Empty;
            ParaValue =String.Empty;
        }

        [DataMember(Order = 0)]
        public string ParameterName { get; set; }
        [DataMember(Order = 1)]
        public string ParaValue { get; set; }

    }
    
    /// <summary>
    /// ReturnString: GetDataSetXml ( Xml string ) ExecNonQuery( Xml string )
    ///               GetMyUtilityFiles( strins ) 
    /// </summary>

    [DataContract]
    public class SvcReturn
    {
        public SvcReturn()
        {
            ReturnCD = String.Empty;
            ReturnMsg = String.Empty;
            ReturnStr = String.Empty;
        }
        public SvcReturn(string returnCD, string returnMsg, string returnStr)
        {
            ReturnCD = returnCD;
            ReturnMsg = returnMsg;
            ReturnStr = returnStr;
        }

        [DataMember(Order = 0)]
        public string ReturnCD { get; set; }  // "FAIL", "OK"

        [DataMember(Order = 1)]
        public string ReturnMsg { get; set; }

        [DataMember(Order = 2)]
        public string ReturnStr { get; set; }
    }

    public class DBOutPut
    {
        public DBOutPut()
        {
            CommandName = String.Empty;
            ParameterName = String.Empty;
            OutValue = String.Empty;
        }
        public int Rowseq { get; set; }
        public string CommandName { get; set; }
        public string ParameterName { get; set; }
        public string OutValue { get; set; }
        public override string ToString()
        {
            string stringValue = "Rowseq: " + Rowseq.ToString();
            stringValue += " CommandName: " + CommandName;
            stringValue += " ParameterName: " + ParameterName;
            stringValue += " OutValue: " + OutValue;

            return stringValue;
        }

    }


}
