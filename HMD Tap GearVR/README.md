# Notes

### Configuration
Hardware: **Samsung Galaxy S8+**, **Gear VR 5** and [Service](https://support.oculus.com/525882800901618/)

Unity: 2017.3.0f3 (64-bit)

Oculus SDK: 1.18.0


### Sensors
Unity **fixes** the acceleration sample rate **only at 60Hz** for **Android**!
> https://docs.unity3d.com/560/Documentation/Manual/MobileInput.html

4kHz Accelerometer
> https://github.com/chemwolf6922/4kHz_accelerometer_MPU9250_STM32)


### Implementation Options
Use Python machine learning library in Unity: [IronPython](http://blog.csdn.net/sinat_32124195/article/details/49366131), [ML-agents](http://blog.csdn.net/ILYPL/article/details/78387390?locationNum=2&fps=1)

Use C# machine learning library: [LibSVMSharp](https://github.com/ccerhan/LibSVMsharp), [Accord.NET](https://github.com/accord-net/framework/wiki/Linear-Support-Vector-Machines)

Communicate with PC -_-|||
