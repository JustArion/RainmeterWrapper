###### ⚙️Requirements:
- [Rainmeter C# API](https://github.com/rainmeter/rainmeter-plugin-sdk/tree/master/API)

###### 📝Usage:
To use the wrapper, simply inherit either [Base Wrapper](https://github.com/Arion-Kun/Rainmeter/blob/main/src/BaseWrapper.cs) or [AbstractWrapper](https://github.com/Arion-Kun/Rainmeter/blob/main/src/AbstractWrapper.cs) and override the base implementation of the method.
`Example:`
```cs
internal sealed class MyImplementation : AbstractWrapper  
{  
 public override void Reload(ref double maxValue)  
    {  
        // Here you can write your code that gets executed on Reload.
    }  
      
  
    public override void CommandReceived(string args)  
    {  
	API.Log(API.LogType.Notice, args);
    }  
  
    protected override void OnUpdate() => Return("Hello");  
}
```
`Note:` The abstract Wrapper can only return string types or number value types that can be converted through the `Convert.ToDouble()` method.

Finally: Build your solution and export the .dll to be converted into a recognized Rainmeter .dll.
A build event can be set up in your project to do this.
```
"$(SolutionDir)\API\DllExporter.exe" "$(ConfigurationName)" "$(PlatformName)" "$(TargetDir)\" "$(TargetFileName)"
```
[Refer to here for a .csproj example of this](https://github.com/rainmeter/rainmeter-plugin-sdk/blob/master/C%23/PluginEmpty/PluginEmpty.csproj#L92)

###### 📝Exposed Wrapper Functions:
```cs
public abstract partial class BaseWrapper
{
	public API API { get; private set; }  
	  
	public virtual void Reload(ref double maxValue) {}  
	  
	public virtual double Update() => 0;  
	  
	public virtual string GetString() => null;  
	  
	/// <summary>  
	/// 'ExecuteBang'  
	/// https://docs.rainmeter.net/developers/plugin/csharp/  
	/// </summary>  
	public virtual void CommandReceived(string args) {}
}
```

```cs
public abstract class AbstractWrapper : BaseWrapper  
{
	protected void Return<T>(T value) where T : notnull

	protected virtual void OnUpdate() {}
}
```

