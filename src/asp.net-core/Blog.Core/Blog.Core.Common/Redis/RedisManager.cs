using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Core.Common
{
    public class RedisManager : IRedisManager
    {
        private readonly string redisConnectionString;

        public volatile ConnectionMultiplexer redisConnection;

        private readonly object redisConnectionLock = new object();

        public RedisManager()
        {
            string redisConfiguration = Appsettings.app(new string[] {

                "AppSettings","RedisCaching","ConnectionString"
            });
            if (string.IsNullOrWhiteSpace(redisConfiguration))
            {
                throw new ArgumentException("redis config is empty", nameof(redisConfiguration));
            }
            this.redisConnectionString = redisConfiguration;
            this.redisConnection = GetRedisConnection();
        }
        private ConnectionMultiplexer GetRedisConnection()
        {
            if (this.redisConnection != null && this.redisConnection.IsConnected)
            {
                return this.redisConnection;
            }
            lock (redisConnectionLock)
            {
                if (this.redisConnection != null)
                {
                    this.redisConnection.Dispose();
                }
                try
                {
                    var config = new ConfigurationOptions
                    {
                        AbortOnConnectFail = false,
                        AllowAdmin = true,
                        ConnectTimeout = 15000,
                        SyncTimeout = 5000,
                        EndPoints = { redisConnectionString }
                    };
                    this.redisConnection = ConnectionMultiplexer.Connect(config);

                }
                catch (Exception)
                {

                    throw;
                }
            }
            return this.redisConnection;
        }

        public void Clear()
        {
            foreach (var endPoint in this.GetRedisConnection().GetEndPoints())
            {
                var server = this.GetRedisConnection().GetServer(endPoint);
                foreach (var key in server.Keys())
                {
                    redisConnection.GetDatabase().KeyDelete(key);
                }
            }
        }

        public TEntity Get<TEntity>(string key)
        {
            var value = redisConnection.GetDatabase().StringGet(key);
            if (value.HasValue)
            {
                //需要用的反序列化，将Redis存储的Byte[]，进行反序列化
                return SerializeHelper.Deserialize<TEntity>(value);
            }
            else
            {
                return default(TEntity);
            }
        }

        public bool Get(string key)
        {
            return redisConnection.GetDatabase().KeyExists(key);
        }

        public string GetValue(string key)
        {
            return redisConnection.GetDatabase().StringGet(key);
        }

        public void Remove(string key)
        {
            redisConnection.GetDatabase().KeyDelete(key);
        }

        public void Set(string key, object value, TimeSpan cacheTime)
        {
            if (value != null)
            {
                //序列化，将object值生成RedisValue
                redisConnection.GetDatabase().StringSet(key, SerializeHelper.Serialize(value), cacheTime);
            }
        }
    }
}
