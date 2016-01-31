using System;

namespace CodeAbility.MonitorAndCommand.Interfaces
{
    public interface IMessageClient
    {
        void Register(string deviceName);

        void Unregister(string deviceName);

        void PublishCommand(string toDevice, string commandTarget, string commandName);

        void PublishData(string toDevice, string dataSource, string dataName);

        void SubscribeToData(string fromDevice, string dataSource, string dataName);

        void SubscribeToCommand(string fromDevice, string commandName, string commandTarget);

        void SubscribeToServerState(string stateName);

        void SendCommand(string toDevice, string commandName, string commandTarget, object commandValue);

        void SendData(string toDevice, string dataSource, string dataName, object dataValue);

    }
}
