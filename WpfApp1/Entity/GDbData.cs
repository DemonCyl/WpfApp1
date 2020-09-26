using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Entity
{
    public class GDbData
    {
        public GDbData(int num = default(int),double torque = default(double), double angle = default(double), string result = default(string))
        {
            Num = num;
            Torque = torque;
            Angle = angle;
            Result = result;
        }

        /// <summary>
        /// 序号
        /// </summary>
        [DisplayName("序号")]
        public int Num { get; set; }
        /// <summary>
        /// 结果
        /// </summary>
        [DisplayName("结果")]
        public string Result { get; set; }
        /// <summary>
        /// 扭矩
        /// </summary>
        [DisplayName("扭矩")]
        public double Torque { get; set; }
        /// <summary>
        /// 角度
        /// </summary>
        [DisplayName("角度")]
        public double Angle { get; set; }

    
    }
}
