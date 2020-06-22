using SqlSugar;
using Student.Achieve.Common.DB;
using System.Collections.Generic;

namespace Student.Achieve.Tran
{
    public class BaseDao<TEntity> where TEntity : class, new()
    {

        public SqlSugar.SqlSugarClient db { get { return GetInstance(); } }
        public void BeginTran()
        {
            db.Ado.BeginTran();
        }
        public void CommitTran()
        {
            db.Ado.CommitTran();
        }
        public void RollbackTran()
        {
            db.Ado.RollbackTran();
        }
        public SqlSugarClient GetInstance()
        {
            SqlSugarClient db = new SqlSugarClient(
                new ConnectionConfig()
                {
                    ConnectionString = BaseDBConfig.ConnectionString,
                    DbType = (DbType)BaseDBConfig.DbType,
                    IsAutoCloseConnection = true,
                    IsShardSameThread = true /*Shard Same Thread*/
                });

            return db;
        }

        public int Add(TEntity entity)
        {

            return db.Insertable(entity).ExecuteReturnIdentity();
        }


        public int Add(List<TEntity> listEntity)
        {

            return db.Insertable(listEntity.ToArray()).ExecuteCommand();
        }


        public int Update(List<TEntity> listEntity)
        {

            return db.Updateable(listEntity.ToArray()).ExecuteCommand();
        }


    }
}
