# Monaco - A Monaco Editor wrapper for WinUI3/WinAppSDK apps

### Usage

* Clone this repository
  
* Add `Monaco.csprog` to your solution project
  
* Add a reference to the `Monaco` project for every project in you want to use the monaco editor
  

### Example code
_See [LightCode](https://github.com/eligamii/LightCode) and the Odyssey browser's Additional DevTools (soon) for real uses_

    XAML
    <!-- Add this to your Page, Window or anything to have access to the editor -->
    xmlns:monaco="using:Monaco"
    <!-- ... -->
    
    <!-- Set the editor language in xaml with StartingLanguage, the only editor property -->
    <monaco:Editor StartingLanguage="CSharp"/>
    
    
    C#
    using Monaco;
    using Monaco.Helpers; // For js string convertion
    ...;
    
    // Set the editor language
    // It's the only thing you can do in the editor before the Loaded event
    // And this can be used only before the Loaded event
    editor.StartingLanguage = Monaco.Language.CSharp; 
    
    editor.Loaded += (sender, args) =>
    {
        // Open a file
        await editor.OpenFileAsync("C:\file.cs", detectLanguage: false);
    
        // Get or set text
        string text = editor.GetTextAsync();
        editor.SetText("this is some text");
    
        // Set the language 
        // 80+ languages are supported in Monaco Editor but only JS, JSON, and HTML seem to have Intellisence
        // Will add Intellisence for other languages in the future
        editor.SetLanguage(Monaco.Language.JavaScript)
    
        // Save to a file
        editor.SaveAsync("C:\file.txt");
        // Save to the file opened with editor.OpenFileAsync()
        editor.SaveAsync();
    
        // Set the editor theme
        // ElementTheme.Light = vs theme, ElementTheme.Dark = vs-dark theme
        editor.RequestedTheme = ElementTheme.Light;
    
        // Control anything in the Monaco Editor with this
        // The example below do the same as the Editor.GetTextAsync()
        // See what you can do with this at https://microsoft.github.io/monaco-editor/docs.html
        string res = await editor.ExecuteAsJSAsync($"editor.getValue()"); // Should return "this is some text" here
        Debug.WriteLine(res);
    }
    
    //  Text changed event
    editor.TextChanged += (sender, text) => Debug.WriteLing(text);
    
    // Link clicked event (if no code is set for this event, the link will open in the default browser)
    editor.LinkClicked += (sender, url) => Debug.WriteLine(url.ToString());
    



### Credit

* Microsoft for WebView2 and the Windows App SDK
  
* The [Monaco Editor](https://github.com/microsoft/monaco-editor) [team](https://github.com/microsoft/monaco-editor/graphs/contributors)
