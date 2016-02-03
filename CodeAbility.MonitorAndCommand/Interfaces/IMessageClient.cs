using System;

namespace CodeAbility.MonitorAndCommand.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IMessageClient
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceName"></param>
        void Register(string deviceName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceName"></param>
        void Unregister(string deviceName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="toDevice"></param>
        /// <param name="commandTarget"></param>
        /// <param name="commandName"></param>
        void PublishCommand(string toDevice, string commandTarget, string commandName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="toDevice"></param>
        /// <param name="dataSource"></param>
        /// <param name="dataName"></param>
        void PublishData(string toDevice, string dataSource, string dataName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fromDevice"></param>
        /// <param name="dataSource"></param>
        /// <param name="dataName"></param>
        void SubscribeToData(string fromDevice, string dataSource, string dataName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fromDevice"></param>
        /// <param name="commandName"></param>
        /// <param name="commandTarget"></param>
        void SubscribeToCommand(string fromDevice, string commandName, string commandTarget);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fromDevice"></param>
        /// <param name="toDevice"></param>
        void SubscribeToTraffic(string fromDevice, string toDevice);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stateName"></param>
        void SubscribeToServerState(string stateName);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="toDevice"></param>
        /// <param name="commandName"></param>
        /// <param name="commandTarget"></param>
        /// <param name="commandValue"></param>
        void SendCommand(string toDevice, string commandName, string commandTarget, object commandValue);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="toDevice"></param>
        /// <param name="dataSource"></param>
        /// <param name="dataName"></param>
        /// <param name="dataValue"></param>
        void SendData(string toDevice, string dataSource, string dataName, object dataValue);

    }
}
