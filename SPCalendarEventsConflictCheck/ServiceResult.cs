using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISAPI.Model
{
    public class ServiceResult<T>
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public T Data { get; set; }

        public static ServiceResult<T> Get(T data)
        {
            return new ServiceResult<T>
            {
                Success = true,
                Data = data
            };
        }

        public static ServiceResult<T> Error(string message)
        {
            return new ServiceResult<T>
            {
                Success = false,
                Data = default(T),
                ErrorMessage = message
            };
        }

        public static ServiceResult<T> Default
        {
            get
            {
                return new ServiceResult<T>
                {
                    Success = false,
                    Data = default(T),
                    ErrorMessage = string.Empty
                };
            }
        }
    }

    public class ServiceResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        public static ServiceResult Sucess
        {
            get
            {
                return new ServiceResult
                {
                    Success = true,
                    ErrorMessage = null,
                    SuccessMessage = null
                };
            }
        }

        public static ServiceResult Error(string message)
        {
            return new ServiceResult
            {
                Success = false,
                ErrorMessage = message
            };
        }
    }
}
