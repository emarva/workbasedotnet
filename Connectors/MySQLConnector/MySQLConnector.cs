﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using MySql.Data.MySqlClient;
using WorkBase.DataLayer;

namespace DevO2.Connectors
{
    internal sealed class MySQLConnector : IConnector
    {
        #region Varibales
        private bool _conectado;
        private bool _tieneFilas;
        private int _filasAfectadas;
        private DlParameterCollection _parametros;

        private DlConnectionString dlCS;
        private MySqlConnection dbCnn;
        private MySqlTransaction dbTrans;
        private List<MySqlCommand> dbCmd = new List<MySqlCommand>();
        private MySqlCommand dbCmdExec;
        private List<MySqlDataAdapter> dbDA = new List<MySqlDataAdapter>();
        private bool disposed;
        #endregion

        #region Constructor
        public MySQLConnector(DlConnectionString cadenaConexion)
        {
            dlCS = cadenaConexion;
        }
        #endregion

        #region Propiedades
        public bool Conectado
        {
            get { return this._conectado; }
        }

        public bool TieneFilas
        {
            get { return this._tieneFilas; }
        }

        public int FilasAfectadas
        {
            get { return this._filasAfectadas; }
        }

        public DlParameterCollection Parametros
        {
            get 
            {
                if (_parametros == null)
                    _parametros = new DlParameterCollection();

                return _parametros; 
            }
            set { _parametros = value; }
        }
        #endregion

        #region Metodos
        public bool Conectar()
        {
            try
            {
                string cadenaConexion = string.Empty;

                if (dlCS.Puerto != 0)
                {
                    cadenaConexion = "Server=" + dlCS.Host + ";Port=" + dlCS.Puerto;
                }
                else
                {
                    cadenaConexion = "Data Source=" + dlCS.Host;
                }

                cadenaConexion += ";Database=" + dlCS.BaseDatos + ";";
                cadenaConexion += "User ID=" + dlCS.Usuario + ";Password=" + dlCS.Contrasena + ";Persist Security Info=False;";

                this.dbCnn = new MySqlConnection(cadenaConexion);
                this.dbCnn.Open();
                this._conectado = true;
                return true;
            }
            catch
            {
                this._conectado = false;
                return false;
            }
        }

        public bool Desconectar()
        {
            try
            {
                this.dbCnn.Close();
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                _conectado = false;
                _parametros = null;
                dbTrans.Dispose();
                dbCmd.Clear();
                dbCmdExec.Dispose();                
            }
        }

        public void CrearBaseDatos(string nombre, Hashtable parametros)
        {

        }

        private MySqlParameter ConvertirParametro(DlParameter parametro)
        {
            MySqlParameter dbParam = new MySqlParameter();

            dbParam.ParameterName = parametro.NombreParam;

            switch (parametro.TipoColumna)
            {
                case DlTipoColumna.BigInt: dbParam.MySqlDbType = MySqlDbType.Int64; break;
                case DlTipoColumna.Binary: dbParam.MySqlDbType = MySqlDbType.Binary; break;
                case DlTipoColumna.Bit: dbParam.MySqlDbType = MySqlDbType.Bit; break;
                case DlTipoColumna.Blob: dbParam.MySqlDbType = MySqlDbType.Blob; break;
                case DlTipoColumna.Byte: dbParam.MySqlDbType = MySqlDbType.Byte; break;
                case DlTipoColumna.Char: dbParam.MySqlDbType = MySqlDbType.String; break;
                case DlTipoColumna.Date: dbParam.MySqlDbType = MySqlDbType.Date; break;
                case DlTipoColumna.DateTime: dbParam.MySqlDbType = MySqlDbType.DateTime; break;
                case DlTipoColumna.Decimal: dbParam.MySqlDbType = MySqlDbType.Decimal; break;
                case DlTipoColumna.Double: dbParam.MySqlDbType = MySqlDbType.Double; break;
                case DlTipoColumna.Float: dbParam.MySqlDbType = MySqlDbType.Float; break;
                case DlTipoColumna.Image: dbParam.MySqlDbType = MySqlDbType.Blob; break;
                case DlTipoColumna.Int: dbParam.MySqlDbType = MySqlDbType.Int32; break;
                case DlTipoColumna.MediumBlob: dbParam.MySqlDbType = MySqlDbType.MediumBlob; break;
                case DlTipoColumna.MediumInt: dbParam.MySqlDbType = MySqlDbType.Int24; break;
                case DlTipoColumna.MediumText: dbParam.MySqlDbType = MySqlDbType.MediumText; break;
                case DlTipoColumna.Money: dbParam.MySqlDbType = MySqlDbType.Decimal; break;
                case DlTipoColumna.NChar: dbParam.MySqlDbType = MySqlDbType.String; break;
                case DlTipoColumna.NVarChar: dbParam.MySqlDbType = MySqlDbType.VarChar; break;
                case DlTipoColumna.LongBlob: dbParam.MySqlDbType = MySqlDbType.LongBlob; break;
                case DlTipoColumna.LongText: dbParam.MySqlDbType = MySqlDbType.LongText; break;
                case DlTipoColumna.Real: dbParam.MySqlDbType = MySqlDbType.Float; break;
                case DlTipoColumna.SmallDateTime: dbParam.MySqlDbType = MySqlDbType.DateTime; break;
                case DlTipoColumna.SmallInt: dbParam.MySqlDbType = MySqlDbType.Int16; break;
                case DlTipoColumna.SmallMoney: dbParam.MySqlDbType = MySqlDbType.Decimal; break;
                case DlTipoColumna.String: dbParam.MySqlDbType = MySqlDbType.String; break;
                case DlTipoColumna.Text: dbParam.MySqlDbType = MySqlDbType.Text; break;
                case DlTipoColumna.Time: dbParam.MySqlDbType = MySqlDbType.Time; break;
                case DlTipoColumna.TimeStamp: dbParam.MySqlDbType = MySqlDbType.Timestamp; break;
                case DlTipoColumna.TinyBlob: dbParam.MySqlDbType = MySqlDbType.TinyBlob; break;
                case DlTipoColumna.TinyInt: dbParam.MySqlDbType = MySqlDbType.Int16; break;
                case DlTipoColumna.TinyText: dbParam.MySqlDbType = MySqlDbType.TinyText; break;
                case DlTipoColumna.Varchar: dbParam.MySqlDbType = MySqlDbType.VarChar; break;
                case DlTipoColumna.VarBinary: dbParam.MySqlDbType = MySqlDbType.VarBinary; break;
                case DlTipoColumna.Xml: dbParam.MySqlDbType = MySqlDbType.Text; break;
            }
            
            if (parametro.Longitud != 0) dbParam.Size = parametro.Longitud;
            if (parametro.Direccion != null) dbParam.Direction = (ParameterDirection)parametro.Direccion;
            if (parametro.ColumnaFuente != string.Empty) dbParam.SourceColumn = parametro.ColumnaFuente;
            if (parametro.Valor != null) dbParam.Value = parametro.Valor;

            return dbParam;
        }

        public DbDataReader ObtenerDataReader(string consulta)
        {
            try
            {
                if (!consulta.ToLower().Contains("select"))
                    return null;

                dbCmdExec = new MySqlCommand(consulta, dbCnn);

                // Si hay parametros los agrego
                if (_parametros != null && _parametros.Count > 0)
                {
                    this.dbCmdExec.CommandType = CommandType.Text;
                    MySqlParameterCollection sqlParams = this.dbCmdExec.Parameters;

                    foreach (DlParameter param in this._parametros)
                    {                        
                        this.dbCmdExec.Parameters.Add(this.ConvertirParametro(param));
                    }
                }

                MySqlDataReader dr = dbCmdExec.ExecuteReader();
                _tieneFilas = dr.HasRows;
                dbCmdExec.Dispose();

                // Filas Afectadas para un SELECT siempre es -1
                _filasAfectadas = -1;
                return dr;
            }
            catch (Exception ex)
            {                
                throw ex;
            }
        }

        public DbDataAdapter ObtenerDataAdapter(string consulta, ref int indiceCmd)
        {
            try
            {
                if (!consulta.ToLower().Contains("select"))
                    return null;

                dbCmd.Add(new MySqlCommand(consulta, dbCnn));
                indiceCmd = dbCmd.Count - 1;

                // Si hay parametros los agrego
                if (_parametros != null)
                {
                    dbCmd[indiceCmd].CommandType = CommandType.Text;
                    MySqlParameterCollection sqlParams = dbCmd[indiceCmd].Parameters;

                    foreach (DlParameter param in this._parametros)
                    {
                        dbCmd[indiceCmd].Parameters.Add(ConvertirParametro(param));
                    }
                }

                dbDA.Add(new MySqlDataAdapter());
                dbDA[indiceCmd].SelectCommand = dbCmd[indiceCmd];
              
                // Filas Afectadas para un SELECT siempre es -1
                this._filasAfectadas = -1;
                return dbDA[indiceCmd];
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void ActualizarObjetoDatos(int indiceCmd, object objetoDatos)
        {
            try
            {
                MySqlCommandBuilder cb = new MySqlCommandBuilder(dbDA[indiceCmd]);
                
                if (objetoDatos.GetType().Name == "DataSet")
                    dbDA[indiceCmd].Update((DataSet)objetoDatos);
                else if (objetoDatos.GetType().Name == "DataTable")
                    dbDA[indiceCmd].Update((DataTable)objetoDatos);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool Ejecutar(string consulta)
        {
            try
            {
                if (consulta.ToLower().Contains("insert") || consulta.ToLower().Contains("update") ||
                    consulta.ToLower().Contains("delete"))
                {
                    this.dbCmdExec = new MySqlCommand(consulta, this.dbCnn);

                    // Si hay parametros los agrego
                    if (this._parametros != null)
                    {
                        this.dbCmdExec.CommandType = CommandType.Text;
                        MySqlParameterCollection sqlParams = this.dbCmdExec.Parameters;

                        foreach (DlParameter param in this._parametros)
                        {
                            this.dbCmdExec.Parameters.Add(this.ConvertirParametro(param));
                        }
                    }

                    if (this.dbTrans != null)
                        if (this.dbTrans.Connection != null)
                            this.dbCmdExec.Transaction = this.dbTrans;

                    // FilasAfectadas con valor 0, significa que no hay filas afectadas o se produjo un error
                    this._filasAfectadas = this.dbCmdExec.ExecuteNonQuery();
                    this.dbCmdExec.Dispose();

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                if (this.dbTrans != null)
                    if (this.dbTrans.Connection != null)
                        this.dbTrans.Rollback();

                // No hay filas
                this._filasAfectadas = -1;

                throw ex;
            }
        }

        public bool IniciarTransaccion()
        {
            try
            {
                this.dbTrans = dbCnn.BeginTransaction();
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool DestinarTransaccion()
        {
            try
            {
                if (this.dbTrans != null)
                    if (this.dbTrans.Connection == null)
                        return false;

                this.dbTrans.Commit();
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool RestaurarTransaccion()
        {
            try
            {
                if (this.dbTrans != null)
                    if (this.dbTrans.Connection == null)
                        return false;

                this.dbTrans.Rollback();
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region IDisposable
        private void Dispose(bool disposing)
        {
            if (disposing && !disposed)
            {
                _parametros.Clear();
                dlCS = null;
                dbCnn.Dispose();
                dbTrans.Dispose();
                dbCmd.Clear();
                dbCmdExec.Dispose();
                dbDA.Clear();
            }

            disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
