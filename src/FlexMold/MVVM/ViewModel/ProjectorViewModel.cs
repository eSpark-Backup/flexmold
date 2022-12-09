using FlexMold.MVVM.Model;
using FlexMold.Utility;
using NLog;
using System;
using System.IO;

namespace FlexMold.MVVM.ViewModel
{
    class ProjectorViewModel
    {
        public Projector MyPorjector { get; set; }
        public ProjectorViewModel(Projector _MyPorjector)
        {
            MyPorjector = _MyPorjector;
        }
        public void sendLasFile(FileInfo _slectedFile, bool _IsSerialMode)
        {
            Logger log = LogManager.GetLogger("Las File Send");
            if (_slectedFile != null)
            {
                if (_slectedFile.FullName.EndsWith(".las"))
                {
                    try
                    {
                        switch (_IsSerialMode)
                        {
                            case true:
                                log.Log(LogLevel.Info, $"Sending file via serial");
                                try
                                {
                                    using (FileStream fs = File.OpenRead(_slectedFile.FullName))
                                    {
                                        log.Log(LogLevel.Info, $"Opening Port {MyPorjector.Port.PortName} Baud rate {MyPorjector.Port.BaudRate}");
                                        MyPorjector.Port.Open();
                                        MyPorjector.Port.Write((new BinaryReader(fs)).ReadBytes((int)fs.Length), 0, (int)fs.Length);
                                        log.Log(LogLevel.Info, $"File Write Sucessfully");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    log.Log(LogLevel.Error, ex, ex.Message);
                                }
                                finally
                                {
                                    MyPorjector.Port.Close();
                                }
                                break;
                            case false:
                                log.Log(LogLevel.Info, $"Sending file via samba");
                                try
                                {
                                    log.Log(LogLevel.Info, $"Connecting to {MyPorjector.IpAddress}");
                                    using (NetworkShareAccesser.Access(MyPorjector.IpAddress, MyPorjector.UserName, MyPorjector.Password))
                                    {
                                        log.Log(LogLevel.Info, $"Credentials Accepted");
                                        log.Log(LogLevel.Info, $"Copying File");
                                        File.Copy(_slectedFile.FullName,
                                            $@"\\{MyPorjector.IpAddress}\{MyPorjector.DataFodlerName}\{_slectedFile.Name}",
                                            true);
                                        log.Log(LogLevel.Info, $"File Copied Sucessfully");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    log.Log(LogLevel.Error, ex, ex.Message);
                                }
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Log(LogLevel.Error, ex, ex.Message);
                        throw;
                    }
                }
                else
                    log.Log(LogLevel.Warn, $"Selected File Not a .las File {_slectedFile.Name}");
            }
            else
                log.Log(LogLevel.Warn, $"Please Select File First");
        }
    }
}
