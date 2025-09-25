# AutoPropertyChangedGenerator

`AutoPropertyChangedGenerator` is a C# source generator that automatically implements `INotifyPropertyChanged` for your properties, reducing boilerplate and making ViewModels cleaner and easier to maintain.

## Features

- Generates property change notification code at compile time.
- Supports both simple auto-properties and proxy properties (for wrapping other objects).


## AutoPropertyChanged/AutoProxyPropertyChanged attribute

>[!NOTE]
> * The class needs the `partial` keyword to allow the generator to add the necessary code.
> * The class must inherit from `ReactiveObject` (in this project, often indirectly through `ViewModelBase`).

Your IDE should warn you if you forget `partial` and there's a custom error message if you forget to inherit from `ReactiveObject` -
but it's easy to miss due to a flood of missing property errors.

### AutoPropertyChanged

For simple auto-properties, create the backing field (e.g., `_myProperty`) and decorate it with the `AutoPropertyChanged` attribute. The generator will create a public property (`MyProperty`) with getter and setter, and the setter will raise the `PropertyChanged` event.

```csharp
public partial class MyViewModel : ViewModelBase
{
	[AutoPropertyChanged]
	private string _myProperty;
}

/* Automatic Generation will create a `public string MyProperty` property.
** The name is the CamelCase version of the variable name (_myProperty in this case).
** The setter will `this.RaisePropertyChanged(nameof(MyProperty))` (if it changed) */
```

### AutoProxyPropertyChanged

For proxy properties, you can use the `AutoProxyPropertyChanged` attribute. This is useful when you want to wrap another object and notify changes on it. Typically used for a Model from qBittorrentClient in this project.
It expects at least one parameter , the name of the property on the wrapped object that you want to notify changes for.
I <u>**highly**</u> recommend using the `nameof` operator to avoid typos and making refactoring easier.

```csharp
public partial class MyViewModel : ViewModelBase
{
	[AutoProxyPropertyChanged(nameof(TorrentInfo.Name))]
	[AutoProxyPropertyChanged(nameof(TorrentInfo.Size), "Bytes")]
	private TorrentInfo _torrentInfo;
}

/* Similarly to the previous example (public) getter and setters for the backing fields will 
** be generated automatically with the setter triggering RaisePropertyChanged.
** For ProxyProperties the name of the proxied property is used,
** unless you assigned a name as per the second decorator above.
** Assigning a name the same way works for regular properties as well, but I would advice not  to use it
** as tools used for refractoring won't pick up on it.
**/
