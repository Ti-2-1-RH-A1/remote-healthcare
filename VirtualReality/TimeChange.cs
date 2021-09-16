using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualReality
{
    class TimeChange
    {
        public float time { get {return time;} set {sendData(time);} }

        public TimeChange(float time)
        {
            this.time = time;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="time"></param>
        
        public void sendData(float time)
        {

        }
        public void sendData(bool staticTime)
        {

        }

        public void SetStatic()
        {
            sendData(true);
        }
        
    }
}
