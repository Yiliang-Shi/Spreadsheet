By:Yiliang Shi and Jet Vellinga
Date:Nov. 4, 2015
Comments: Depends on PS4 for the spreadsheet, which in turn depends on PS2 and PS3 for dependencyGraph and Formula respectively.
 Uses MVC architecture. The view class represents the view component, the controller class represents the control component,
 and the spreadsheet class represents the model component.

 The main entry point (program.cs) contains the application context class, which creates the view, which then creates the controller.
 The controller then makes references to the view and creates a new instance of the spreadsheet object.

 The view contains all the event handling methods as well as some simple form updating and error checking. However, the view passes most
 of the implementation to the controller. The controller makes use of the spreadsheet properties and methods to display the correct data in the display.
 The controller also queries and updates the spreadsheet model as needed.


 When a key with a char output is typed when focus is on the spreadsheet panel, focus shifts to the content box and the typed character is shown.
 
 
 Tested using the Coded UI Test Builder as well as user operated tests. 
	Tried to analyze the code coverage, but the UI Tests cannot access the private methods so the can't actually test the code.
	However, we did test assertions to ensure that the spreadsheet GUI functioned as expected

	Most tests were user-operated as they allowed us to vary inputs and test more things. We also had people who are completely unfamiliar
	with the code and the API interact with the GUI to increase the chance that they would test something unexpected that we had not 
	considered during implementation.
	
	Due to the difficulty of combining multiple test projects, two separate UI tests are included in the project.
	In addition, in order for the tests to run, spreadsheetGui must be running. A few tests must be started individually 
	as they are dependent on the default output when a form is opened. 


 
 Additional features:
 The Spreadsheet GUI allows the user to save the spreadsheet as a .xls file, that can then be opened in Excel.
 When the user saves a spreadsheet for the first time or chooses to save as, they are given the option to save as excel file type .xls
 The file type is checked when the save file dialog is completed and the spreadsheet is then saved accordingly. If the file type is .sprd,
 it is saved using the spreadsheet save function. Otherwise it is saved using a new method in the controller that writes the file in excel readable format.


 Version: Uses updated version of dependency graph in PS2 due to an error in the replace class.


Websites referenced:
For the additional feature of exporting to excel, instructions on creating an excel file with c# was found at
http://csharp.net-informations.com/excel/csharp-create-excel.htm

Stack overflow was consulted in implementing keyboard bindings and bypassing the default input classification for non character keys. 

http://stackoverflow.com/questions/3380662/is-there-a-way-to-have-the-close-window-button-x-call-a-method-before-actually

https://msdn.microsoft.com/en-us/library/system.windows.forms.savefiledialog(v=vs.110).aspx

http://stackoverflow.com/questions/5136254/saving-file-using-savefiledialog

https://github.com/uofu-cs3500-fall15/Clue/blob/master/PS4/Spreadsheet/Spreadsheet.cs