using CoreWCF;
using MySql.Data.MySqlClient;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml;
using System.Xml.Serialization;

namespace BBS
{
    
    public class DBService : IDBService
    {
        private const string sOkMsg1 = "Completed successfully";
        
        /// <summary>
        /// DB Output List
        /// </summary>
        private List<DBOutPut> OutputList { get; set; }= new List<DBOutPut>();
        public DBService()
        {
            Console.WriteLine("DBService Created...");
        }
        public SvcReturn ExecNonQuery(MyCommand[] myCmds)
        {
            try
            {
                using (TransactionScope scope = new( TransactionScopeOption.Required ))
                {
                    DBExecManager dBExec = new DBExecManager (myCmds);

                    foreach (KeyValuePair<MyCommand, IDbCommand> keyValuePair in dBExec.DicDBCmd)
                    {
                        //
                        MyCommand myCmd = keyValuePair.Key;
                        IDbCommand dbCommand = keyValuePair.Value;

                        // Parameter가 없을 경우 바로 실행
                        if (dbCommand.Parameters.Count <= 0)
                        {
                            dbCommand.ExecuteNonQuery();
                            continue;
                        }

                        if (myCmd.ParaValues == null) throw new Exception("MyCommand Parameter value is null error");

                        // Parameter가 있을 경우
                        int iValeuSetCnt = myCmd.ParaValues.Length;
                        
                        // 한개의 db command별로 여러번 실행, 
                        for (int iSetCnt = 0; iSetCnt < iValeuSetCnt; iSetCnt++)
                        {
                            // To set Parameter Value 
                            SetParaValue(dbCommand, myCmd, iSetCnt);
                            dbCommand.ExecuteNonQuery();
                            ListAddOutput(dbCommand, myCmd.CommandName );
                            //ExecNonQuery(dbCommand, myCmd.CommandName);
                        }
                    } // exec foreach dbcmd
                    
                    // commit commands
                    scope.Complete();
                } // end using transactionscope
                
            }
            catch (Exception ex)
            {

                throw new FaultException(ex.Message); 
            }

            SvcReturn rtnData = new()
            {
                ReturnCD = "OK",
                ReturnMsg = sOkMsg1,
                ReturnStr = MyDbStatic.ToXML(OutputList, "Result_Ds")
            };
            return rtnData;

        }

        /// <summary>
        /// ExecNonQuery 실행 전 parameter Value처리
        /// </summary>
        private void SetParaValue(IDbCommand dbCommand, MyCommand myCmd, int iSetSeq)
        {
            if (dbCommand.Parameters == null) return;
            if (dbCommand.Parameters.Count <= 0) return;

            // Set Parameter Value
            int iCol = 0;
            foreach (IDbDataParameter para in dbCommand.Parameters)
            {
                // 파라미터가  input,  InputOutput일 경우 값을 저장
                if (para.Direction == ParameterDirection.Input ||
                    para.Direction == ParameterDirection.InputOutput )
                {
                    para.Value = myCmd?.ParaValues?[iSetSeq][iCol].ParaValue;
                }
                iCol++;

            }
            // Header 값 참조 파라미터 값 설정
            foreach (MyPara para in myCmd.Parameters ?? Enumerable.Empty<MyPara>())
            {
                if (string.IsNullOrEmpty(para.HeaderCommandName)) continue;
                if (string.IsNullOrEmpty(para.HeaderParameter)) continue;

                if (((IDbDataParameter)dbCommand.Parameters[para.ParameterName]).Direction == ParameterDirection.Input ||
                    ((IDbDataParameter)dbCommand.Parameters[para.ParameterName]).Direction == ParameterDirection.InputOutput)
                {

                    DBOutPut? output = OutputList.OrderByDescending(x => x.Rowseq)
                                                 .FirstOrDefault(x => x.CommandName == para.HeaderCommandName &&
                                                                      x.ParameterName == para.ParameterName);

                    if (output == null) throw new Exception("There is nothing to refer header output value");
                    
                    ((IDbDataParameter)dbCommand.Parameters[para.ParameterName]).Value = output.OutValue;
                }
            }
        } //SetParaValue

        /// <summary>
        /// DB ExecNonQuery 실행 후 필요한 파라미터 저장
        /// output,InputOutput, return value 저장
        /// </summary>
        /// <param name="dbCommand"></param>
        /// <param name="myCmdName"></param>
        private void ListAddOutput(IDbCommand dbCommand, string myCmdName)
        {
            try
            {
                foreach (IDbDataParameter param in dbCommand.Parameters)
                {
                    if (param.Direction == ParameterDirection.Output ||
                        param.Direction == ParameterDirection.InputOutput ||
                        param.Direction == ParameterDirection.ReturnValue)
                    {

                        DBOutPut rtnData = new()
                        {
                            Rowseq = OutputList.Count + 1,
                            CommandName = myCmdName,
                            ParameterName = param.ParameterName.Trim(),
                            OutValue = param.Value?.ToString() ?? String.Empty
                        };

                        OutputList.Add(rtnData);
                        Console.WriteLine(rtnData);
                    }
                }

            }
            catch (Exception)
            {
                throw;
                
            }
        }

        public SvcReturn GetDataSetXml(MyCommand myCmd)
        {
            try
            {
                DBFillManager dBFill = new DBFillManager(myCmd) ?? throw new Exception (" New DBFillManager Error ");

                if (dBFill.DbCmd == null)   throw new Exception(" New DBFillManager Command Error ");

                // Set Parameter Value
                int iCol = 0;
                foreach (IDbDataParameter para in dBFill.DbCmd.Parameters)
                {
                    if (para.Direction == ParameterDirection.InputOutput) continue;
                    if (para.Direction == ParameterDirection.Output) continue;

                    // 조회는 한번의 첫 번째 파라미터 셋트만 실행
                    para.Value = myCmd?.ParaValues?[0][iCol].ParaValue;
                    iCol++;
                }

                string  xmlString = dBFill.ExecDBFill(dBFill.DbCmd);
                
                return new SvcReturn {
                    ReturnCD = "OK",
                    ReturnMsg = sOkMsg1,
                    ReturnStr = xmlString
                };
                
            }
            catch (Exception ex)
            {
                throw new FaultException(ex.Message);
                
            }
        }

    }

    /// <summary>
    /// GetDataSetXml 용 Class
    /// </summary>
    public class DBFillManager
    {
        private ConnectionStringSettings? DbConnSettings { get; set; } // = ConfigurationManager.ConnectionStrings[myCmd.ConnectionName];
        private IDbConnection? DbConn { get; set; }

        public IDbCommand? DbCmd { get; set; }

        public DBFillManager(MyCommand myCmd)
        {
            try
            {
                DbConnSettings = ConfigurationManager.ConnectionStrings[myCmd.ConnectionName];
                // Create DB Connection 
                DbConn = MyDbStatic.CreateConnection(DbConnSettings) ?? throw new Exception("Db Connection Error ");

                DbConn.Open();

                // Create DB Command
                DbCmd = DbConn.CreateCommand();
                DbCmd.Connection = DbConn;
                DbCmd.CommandType = (CommandType)myCmd.CommandType;
                DbCmd.CommandText = myCmd.CommandText;

                // Add Parameter
                for (int i = 0; i < myCmd.Parameters?.Length; i++)
                {
                    if (string.IsNullOrEmpty(myCmd.Parameters[i].ParameterName)) break;
                    if (string.IsNullOrWhiteSpace(myCmd.Parameters[i].ParameterName)) break;

                    IDbDataParameter? dbDataParameter = MyDbStatic.CreateParameter(myCmd.Parameters[i], DbConnSettings.ProviderName);

                    if (dbDataParameter == null) throw new Exception("Create DB Parameter Fail");
                    DbCmd.Parameters.Add(dbDataParameter);

                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public string ExecDBFill(IDbCommand dbCommand)
        {

            try
            {
                if (DbConnSettings == null) throw new Exception("DB Fill Manager DB Connection string Error");
                
                // DB Adapter
                IDataAdapter? dbAdapter = MyDbStatic.CreateDbAdapter(dbCommand, DbConnSettings.ProviderName);

                if (dbAdapter == null) throw new Exception("Create DB Adapter Fail");

                // DB Command exec 
                DataSet? ds = new("Result_Ds");
                dbAdapter.Fill(ds);

                // 쿼리 데이터셋 to Xml string  
                System.IO.StringWriter writer = new();
                ds.WriteXml(writer, XmlWriteMode.WriteSchema);

                return writer.ToString();

                
            }
            catch (Exception)
            {

                throw;
            }

       
        }

    }// class end

    public class DBExecManager
    {
        private Dictionary<string, IDbConnection> DicDBConn { get; set; } = new();
        public Dictionary<MyCommand, IDbCommand> DicDBCmd { get; set; } = new();
        public DBExecManager(MyCommand[] myCmds)
        {
            try
            {
                // Create Connection Dictionary
                foreach (MyCommand myCmd in myCmds)
                {
                    // 해당 연결이 있어면 Skip
                    if (DicDBConn.ContainsKey(myCmd.ConnectionName)) continue;

                    ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings[myCmd.ConnectionName];
                    IDbConnection? dbconnection = MyDbStatic.CreateConnection(settings);

                    // DB 연결을 만들지 못하면 Exception처리 
                    if (dbconnection == null) throw new Exception("Create DB Connection Fail");

                    dbconnection.Open();
                    DicDBConn.Add(myCmd.ConnectionName, dbconnection);
                }

                // Create DB Command
                foreach (MyCommand myCmd in myCmds)
                {
                    ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings[myCmd.ConnectionName];
                    IDbConnection conn = DicDBConn[myCmd.ConnectionName];

                    IDbCommand dbCommand = conn.CreateCommand();
                    dbCommand.Connection = conn;
                    dbCommand.CommandType = (CommandType)myCmd.CommandType;
                    dbCommand.CommandText = myCmd.CommandText;
                    if (settings.ProviderName.Equals("Oracle.ManagedDataAccess.Client"))
                        ((OracleCommand)dbCommand).BindByName = true;

                    DicDBCmd.Add(myCmd, dbCommand);

                    if (myCmd.Parameters == null) continue;
                    
                    // Create DB Parameter
                    for (int i = 0; i < myCmd.Parameters?.Length; i++)
                    {
                        if (string.IsNullOrEmpty(myCmd.Parameters[i].ParameterName)) break;
                        if (string.IsNullOrWhiteSpace(myCmd.Parameters[i].ParameterName)) break;

                        IDbDataParameter? dbDataParameter = MyDbStatic.CreateParameter(myCmd.Parameters[i], settings.ProviderName);
                        if (dbDataParameter == null) throw new Exception("Create DB Parameter Fail");
                        dbCommand.Parameters.Add(dbDataParameter);
                    }
                }

            }
            catch (Exception)
            {

                throw;
            }

        }
        
    }
    public static class MyDbStatic
    {
        public static IDbConnection? CreateConnection(ConnectionStringSettings settings)
        {

            if (settings.ProviderName.Equals("System.Data.SqlClient"))
                return new SqlConnection(settings.ConnectionString);
            else if (settings.ProviderName.Equals("Oracle.ManagedDataAccess.Client"))
                return new OracleConnection(settings.ConnectionString);
            else if (settings.ProviderName.Equals("MySql.Data.MySqlClient"))
                return new MySqlConnection(settings.ConnectionString);
            else
                return null;
        }
        public static IDbDataParameter? CreateParameter(MyPara myPara, string sProviderName)
        {

            IDbDataParameter dbDataParameter;

            if (string.IsNullOrEmpty(myPara.ParameterName)) return null;
            if (string.IsNullOrWhiteSpace(myPara.ParameterName)) return null;

            if (sProviderName.Equals("System.Data.SqlClient"))
                dbDataParameter = new SqlParameter(myPara.ParameterName, (SqlDbType)myPara.DbDataType);
            else if (sProviderName.Equals("Oracle.ManagedDataAccess.Client"))
                dbDataParameter = new OracleParameter(myPara.ParameterName, (OracleDbType)myPara.DbDataType);
            else if (sProviderName.Equals("MySql.Data.MySqlClient"))
                dbDataParameter = new MySqlParameter(myPara.ParameterName, (MySqlDbType)myPara.DbDataType);
            else
                return null;

            dbDataParameter.Direction = (ParameterDirection)Convert.ToInt32(myPara.Direction);

            return dbDataParameter;

        }

        public static IDbDataAdapter? CreateDbAdapter(IDbCommand dbcmd, string sProviderName)
        {
            IDbDataAdapter dbAdapter;

            if (sProviderName.Equals("System.Data.SqlClient"))
                dbAdapter = new SqlDataAdapter((SqlCommand)dbcmd);
            else if (sProviderName.Equals("Oracle.ManagedDataAccess.Client"))
                dbAdapter = new OracleDataAdapter((OracleCommand)dbcmd);
            else if (sProviderName.Equals("MySql.Data.MySqlClient"))
                dbAdapter = new MySqlDataAdapter((MySqlCommand)dbcmd);
            else
                return null;

            return dbAdapter;
        }

        public static string ToXML<T>(List<T> outList, string rootName)
        {
            XmlSerializer serializer = new(typeof(List<T>), new XmlRootAttribute(rootName));

            // Remove Declaration
            var settings = new XmlWriterSettings
            {
                Indent = true,
                OmitXmlDeclaration = true
            };

            // Remove Namespace
            var xmlNs = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });

            using (var swr = new StringWriter())
            using (XmlWriter writer = XmlWriter.Create(swr, settings))
            {
                serializer.Serialize(writer, outList, xmlNs);
                return swr.ToString();
            }

        }

    }

}
