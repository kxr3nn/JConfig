# JConfig
### Lightweight and compact library for convenient and easy work with Json files
I needed an xml replacement, but I didn't want to use yaml.
This is how Json `with comment support` turned out :)

## Usage
Create an instance of the JConfig class by passing the `path to the file` and the `class type` to the constructor:
```C#
JConfig config = new JConfig("Info.json", typeof(ExampleConfigClass));
```
JConfig automatically deserializes the file. Now we can get deserialized data:
```C#
ExampleConfigClass configData = (ExampleConfigClass)config.GetData();
```
If we have changed the any values in the class, we can easily write it to a json file:
```C#
configData.AnyField = "example";
config.Update();
```
If we have changed the json file itself, and we want to use the `updated data from json without restarting` the application, we can use:
```C#
config.Load();
```

### JConfig also supports comments in json files. 
 Just write ``//this is comment, dude`` anywhere in your json file
