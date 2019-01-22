using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilverTest.libs
{
    public class GIDMaker
    {
        private  static GIDMaker onlyme = null;
        int newmaxid = 0;
        int samplemaxid = 0;
        private GIDMaker()
        {
            //talbe中item的唯一全局号
            newmaxid = int.Parse(Utility.GetValueFrXml("/config/QM201H/wavehistory/fileid", "newsample"));
            samplemaxid = int.Parse(Utility.GetValueFrXml("/config/QM201H/wavehistory/fileid", "standardsample")); ;
        }
        public static GIDMaker GetMaker()
        {
            if(onlyme == null)
            {
                onlyme = new GIDMaker();
                return onlyme;
            }
            else
            {
                return onlyme;
            }
        }
        //获取newtable中itemid号
        public string GetNId()
        {
            string t = "N";
            newmaxid++;
            Utility.SetValueToXml("/config/QM201H/wavehistory/fileid", "newsample", newmaxid.ToString());
            if (newmaxid.ToString().Length < 5)
            {
                for(int i=0; i < 4 - newmaxid.ToString().Length; i++)
                {
                    t += '0';
                }
            }
            t += newmaxid.ToString();

            return t;
        }
        //获取sampletable中itemid号
        public string GetSId()
        {
            string t = "S";
            samplemaxid++;
            Utility.SetValueToXml("/config/QM201H/wavehistory/fileid", "standardsample", samplemaxid.ToString());
            if (samplemaxid.ToString().Length < 5)
            {
                for (int i = 0; i < 4 - samplemaxid.ToString().Length; i++)
                {
                    t += '0';
                }
            }
            return t + samplemaxid.ToString();
            ;
        }
    }
}
