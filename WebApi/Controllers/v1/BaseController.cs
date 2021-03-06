using Common;
using Medallion.Threading;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Models.Dtos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WebApi.Libraries;

namespace WebApi.Controllers.v1
{
    /// <summary>
    /// 系统基础方法控制器
    /// </summary>
    [ApiVersion("1")]
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController : ControllerCore
    {



        /// <summary>
        /// 获取省市级联地址数据
        /// </summary>
        /// <param name="provinceId">省份ID</param>
        /// <param name="cityId">城市ID</param>
        /// <returns></returns>
        /// <remarks>不传递任何参数返回省份数据，传入省份ID返回城市数据，传入城市ID返回区域数据</remarks>
        [HttpGet("GetRegion")]
        public List<dtoKeyValue> GetRegion(int provinceId, int cityId)
        {
            var list = new List<dtoKeyValue>();

            if (provinceId == 0 && cityId == 0)
            {
                list = db.TRegionProvince.Select(t => new dtoKeyValue { Key = t.Id, Value = t.Province }).ToList();
            }

            if (provinceId != 0)
            {
                list = db.TRegionCity.Where(t => t.ProvinceId == provinceId).Select(t => new dtoKeyValue { Key = t.Id, Value = t.City }).ToList();
            }

            if (cityId != 0)
            {
                list = db.TRegionArea.Where(t => t.CityId == cityId).Select(t => new dtoKeyValue { Key = t.Id, Value = t.Area }).ToList();
            }

            return list;
        }



        /// <summary>
        /// 获取全部省市级联地址数据
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetRegionAll")]
        public List<dtoKeyValueChild> GetRegionAll()
        {

            var list = db.TRegionProvince.Select(t => new dtoKeyValueChild
            {
                Key = t.Id,
                Value = t.Province,
                ChildList = t.TRegionCity.Select(c => new dtoKeyValueChild
                {
                    Key = c.Id,
                    Value = c.City,
                    ChildList = c.TRegionArea.Select(a => new dtoKeyValueChild
                    {
                        Key = a.Id,
                        Value = a.Area
                    }).ToList()
                }).ToList()
            }).ToList();

            return list;
        }



        /// <summary>
        /// 自定义二维码生成方法
        /// </summary>
        /// <param name="text">数据内容</param>
        /// <returns></returns>
        [HttpGet("GetQrCode")]
        public FileResult GetQrCode(string text)
        {
            var image = ImgHelper.GetQrCode(text);

            using (var ms = new MemoryStream())
            {
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                return File(ms.ToArray(), "image/png");
            }
        }



        /// <summary>
        /// 获取指定组ID的KV键值对
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        [HttpGet("GetValueList")]
        public List<dtoKeyValue> GetValueList(Guid groupId)
        {

            var list = db.TAppSetting.Where(t => t.IsDelete == false & t.Module == "Dictionary" & t.GroupId == groupId).Select(t => new dtoKeyValue
            {
                Key = t.Key,
                Value = t.Value
            }).ToList();

            return list;
        }



        /// <summary>
        /// 获取一个雪花ID
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetSnowflakeId")]
        public long GetSnowflakeId()
        {
            return HttpContext.RequestServices.GetService<SnowflakeHelper>().GetId();
        }




        /// <summary>
        /// 发送一个CAP消息
        /// </summary>
        /// <returns></returns>
        [HttpGet("ShowMessage")]
        public bool ShowMessage(string msg)
        {

            cap.Publish("ShowMessage", msg);

            return true;
        }




        /// <summary>
        /// 分布式锁demo
        /// </summary>
        /// <returns></returns>
        [HttpGet("DistLock")]
        public bool DistLock()
        {

            //互斥锁
            using (distLock.AcquireLock(""))
            {
            }

            //互斥锁，可配置内部代码最多同时运行数
            using (distSemaphoreLock.AcquireSemaphore("", 5))
            {
            }

            //读锁，多人同读，与写锁互斥
            using (distUpgradeableLock.AcquireReadLock(""))
            {
            }


            //写锁，互斥
            using (distUpgradeableLock.AcquireWriteLock(""))
            {
            }

            //可升级读锁，初始状态为读锁，可手动升级为写锁
            using (var handle = distUpgradeableLock.AcquireUpgradeableReadLock(""))
            {

                //升级写锁
                handle.UpgradeToWriteLock();
            }

            return true;
        }


    }
}