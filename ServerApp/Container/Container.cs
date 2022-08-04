using System;
using ConsoleApp;
using Contracts;

public class Container : IContainer
{
    private Program program;
    public void Init()
    {
        program = new Program();
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