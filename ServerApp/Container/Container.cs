using System;
using ConsoleApp;
using Contracts.XFlow.Container;
using Contracts.XFlow.Container.Host;


public class Container: IContainer
{
    private Program program;
    
    IHost host;

    public Container(IHost host)
    {
        this.host = host;
    }
    
    public void Init()
    {
        program = new Program(host.GetConfig(), host.GetLogger());
    }

    public void Start()
    {
        program.Run();
    }

    public void Update()
    {
        throw new NotImplementedException();
    }

    public void Stop()
    {
        program.Stop();
    }

    public string GetInfo()
    {
        return program.GetInfo();
    }
}