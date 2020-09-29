using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;
using System.Text.RegularExpressions;

namespace water
{
    class Email
    {
        string m_user = "";
        string m_passwd = "";
        string m_mail = "";
        int m_port = 110;
        TcpClient mailclient;
        Boolean m_status = false;
        SslStream sslStream = null;

        public Email(string _user, string _passwd, string _mail, int _port = 110)
        {
            m_user = _user;
            m_passwd = _passwd;
            m_mail = _mail;
            m_port = _port;
            if (connect()) m_status = true;
        }

        public Boolean status()
        {
            if (mailclient.Connected) return true; else return false;

        }

        public int m_newmess()
        {
            if (status())
            {
                //sw.WriteLine("STAT"); //Send stat command to get number of messages
               // sw.Flush();

                //string response = sr.ReadLine();

                //find number of message

                sslStream.Write(Encoding.ASCII.GetBytes("STAT\r\n"));
                byte[] buffer = new byte[2048];
                int bytes = sslStream.Read(buffer, 0, buffer.Length);
                var response = Encoding.ASCII.GetString(buffer, 0, bytes);
                string[] nummess = response.Split(' ');
                nummess[1] = "0" + nummess[1];
                return(Convert.ToInt16(nummess[1]));
            }
            else return -1;
        }

        public string Read(int count = 1)
        {
            if (status())
            {
                byte[] buffer = new byte[2048];
                int bytes = -1;

                //sslStream.Write(Encoding.ASCII.GetBytes("UIDL " + count.ToString() + "\r\n"));
                //bytes = sslStream.Read(buffer, 0, buffer.Length);
                //var response = Encoding.ASCII.GetString(buffer, 0, bytes);
                //int ll = response.LastIndexOf(" ");
                //response = response.Substring(ll,response.Length-ll-2);
                count--;
                sslStream.Write(Encoding.ASCII.GetBytes("RETR "+count.ToString()+"\r\n"));
                bytes = sslStream.Read(buffer, 0, buffer.Length);
                var response = Encoding.ASCII.GetString(buffer, 0, bytes);
                bytes = Convert.ToInt32(response.Substring(response.IndexOf(" "), response.LastIndexOf(" ") - response.IndexOf(" ")));
                buffer = new byte[bytes];
                sslStream.Write(Encoding.ASCII.GetBytes("RETR " + count.ToString() + "\r\n"));
                bytes = sslStream.Read(buffer, 0, buffer.Length);
                response += Encoding.ASCII.GetString(buffer, 0, bytes);
                
                 if (response.IndexOf("attachment") != -1)
                {
                     StoreAttachments(response);
                }
                return response; 
            }
            else return "";
        }

        private void StoreAttachments(string txtResponse)
        {
            List<string> lstFilename = new List<string>();

            Regex reFilename = new Regex(@"\bfilename=\""(.*)\b");
            MatchCollection mFilename = reFilename.Matches(txtResponse);

            foreach (Match mf in mFilename)
            {
                lstFilename.Add(mf.Groups[1].Value);
            }
            foreach (string strfileName in lstFilename)
            {
                writeFile(strfileName, parseValue(txtResponse, @"Content-Disposition: attachment; filename=""" + strfileName + "\"", "--", false));
            }
        }

        private string parseValue(String responseBody, String prefix, String posix, bool include1)
        {

            String action;

            int i;
            //try to find the first type

            i = responseBody.IndexOf(prefix);

            if (i >= 0)
            {

                if (include1)
                {

                    action = responseBody.Substring(i);

                }

                else
                {

                    action = responseBody.Substring(i + prefix.Length);

                }

                if (!posix.Equals(""))
                    action = action.Substring(0, action.IndexOf(posix));

                return action.Trim();
            }

            throw new Exception("Value '" + prefix + "' not found");
        } 

        private void writeFile(string fileName, string fileContent)
        {
            try
            {
                FileStream fs = new FileStream(@"c:\" + fileName, FileMode.CreateNew); fs.Write(Convert.FromBase64String(fileContent), 0,
                               Convert.FromBase64String(fileContent).Length);
                fs.Close();
            }
            catch (Exception ex)
            {
            }
        }

        public Boolean disconnect()
        {
            if (status())
            {
                try
                {
                    sslStream.Write(Encoding.ASCII.GetBytes("QUIT\r\n"));
                    mailclient.Close();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }

        public Boolean connect()
        {
            
            Boolean result = false;
            int bytes = -1;
            try
            {
                mailclient = new TcpClient(m_mail, m_port);
            }
            catch (SocketException ExTrhown)
            {
                result = false;
                throw new Exception(ExTrhown.Message + "Unable to connect to server 1");
            }

            //if (cbSsl.Checked)
            //{
            //    NetStrm = new SslStream(mailclient.GetStream());
            //    NetStrm.AuthenticateAsClient("pop.orange.fr");
            //    sr = new StreamReader(NetStrm);
            //    sw = new StreamWriter(NetStrm);
            //}
            //else
            //{
            sslStream = new SslStream(mailclient.GetStream());
            sslStream.AuthenticateAsClient(m_mail);

            byte[] buffer = new byte[2048];
            bytes = sslStream.Read(buffer, 0, buffer.Length);
            var response = Encoding.ASCII.GetString(buffer, 0, bytes);
            if (response.Contains("-ERR"))
            {
                result = false;
            }

            //Логинимся - тут все нормально
            sslStream.Write(Encoding.ASCII.GetBytes("USER "+m_user+"\r\n"));
            bytes = sslStream.Read(buffer, 0, buffer.Length);
            response = " "+Encoding.ASCII.GetString(buffer, 0, bytes);
            if (response.Contains("-ERR"))
            {
                result = false;
            }

            //Пароль - тут тоже все работает
            sslStream.Write(Encoding.ASCII.GetBytes("PASS "+m_passwd+"\r\n"));
            bytes = sslStream.Read(buffer, 0, buffer.Length);
            response = " "+Encoding.ASCII.GetString(buffer, 0, bytes);
            if (response.Contains("-ERR"))
            {
                result = false;
            }

            return result;
        }
    }
}
