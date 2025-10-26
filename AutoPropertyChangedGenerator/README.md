# AutoPropertyChangedGenerator

`AutoPropertyChangedGenerator` is a C# source generator that automatically implements `INotifyPropertyChanged` for your properties, reducing boilerplate and making ViewModels cleaner and easier to maintain.

## Features

* Generates public getter/setter, with `this.RaisePropertyChanged()` for the setter
* Supports:
  * Regular properties  
    ```csharp
    [AutoPropertyChanged]
    private string _myProperty;
    ```
  * Proxy properties (and multiple properties, with a custom name if desired)  
    ```csharp
    [AutoProxyPropertyChanged(nameof(TorrentInfo.Name))]
    [AutoProxyPropertyChanged(nameof(TorrentInfo.Size), "Bytes")]
    private TorrentInfo _torrentInfo;
    ```
  * Dependent property notifications
    ```csharp
    [AutoPropertyChanged]
    [AlsoNotify(nameof(FullName))]
    [AlsoNotify(nameof(DisplayText))]
    private string _firstName;
    ```

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

It expects at least one parameter, the name of the property on the wrapped object that you want to notify changes for.

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
** Although name customization is an option I would recommend using it,
** tools used for refactoring are unlikely to figure out what to do with a string.*/
```

### AlsoNotify

The `AlsoNotify` attribute allows you to notify additional properties when a field changes. This is useful for computed properties that depend on other properties.

I <u>**highly**</u> recommend using the `nameof` operator to avoid typos and making refactoring easier.

```csharp
public partial class MyViewModel : ViewModelBase
{
    [AutoPropertyChanged]
    [AlsoNotify(nameof(FullName))]
    [AlsoNotify(nameof(InitialsWithName))]
    private string _firstName;

    [AutoPropertyChanged]
    [AlsoNotify(nameof(FullName))]
    [AlsoNotify(nameof(InitialsWithName))]
    private string _lastName;

    // Computed properties that depend on FirstName and LastName
    public string FullName => $"{FirstName} {LastName}";
    public string InitialsWithName => $"{FirstName[0]}{LastName[0]} - {FullName}";
}
/* When FirstName or LastName changes, the setter will automatically call:
** - this.RaisePropertyChanged(nameof(FirstName)) or this.RaisePropertyChanged(nameof(LastName))
** - this.RaisePropertyChanged(nameof(FullName))
** - this.RaisePropertyChanged(nameof(InitialsWithName))
** This ensures the UI updates all dependent properties. */
```

#### Using AlsoNotify with Proxy Properties

You can also use `AlsoNotify` with proxy properties:

```csharp
public partial class TorrentViewModel : ViewModelBase
{
    [AutoProxyPropertyChanged(nameof(TorrentInfo.Downloaded))]
    [AutoProxyPropertyChanged(nameof(TorrentInfo.Size))]
    [AlsoNotify(nameof(ProgressPercentage))]
    [AlsoNotify(nameof(RemainingBytes))]
    private TorrentInfo _torrentInfo;

    // Computed properties based on the proxy properties
    public double ProgressPercentage => Size > 0 ? (Downloaded / (double)Size) * 100 : 0;
    public long RemainingBytes => Size - Downloaded;
}
/* When Downloaded or Size changes through the proxy properties,
** both ProgressPercentage and RemainingBytes will be notified to update. */
```

## Example: Complete ViewModel

Here's a complete example showing all features together:

```csharp
public partial class PersonViewModel : ViewModelBase
{
    // Simple property
    [AutoPropertyChanged]
    private int _age;

    // Properties with dependent notifications
    [AutoPropertyChanged]
    [AlsoNotify(nameof(FullName))]
    [AlsoNotify(nameof(DisplayText))]
    private string _firstName;

    [AutoPropertyChanged]
    [AlsoNotify(nameof(FullName))]
    [AlsoNotify(nameof(DisplayText))]
    private string _lastName;

    // Proxy properties with dependent notifications
    [AutoProxyPropertyChanged(nameof(Address.Street))]
    [AutoProxyPropertyChanged(nameof(Address.City))]
    [AutoProxyPropertyChanged(nameof(Address.ZipCode))]
    [AlsoNotify(nameof(FullAddress))]
    private Address _address;

    // Computed properties
    public string FullName => $"{FirstName} {LastName}";
    public string DisplayText => $"{FullName} (Age: {Age})";
    public string FullAddress => $"{Street}, {City} {ZipCode}";
}
```