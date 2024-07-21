
Main Features (60 marks)
• Clock
    o Show current Day, Date, Time                                                            [IMPLEMENTED on MainPage page 1] [Day shown [Mon-Sun] , Date shown[DD/MM/YYYY] , Time shown[HH:MM AMPM]]
    o Customizable Greeting Text (Settings Page/Text file/JSON/XML)                           [IMPLEMENTED on MainPage page 1 and greetings.txt]
    o Show Current Weather Summary (At least 3 weather information)                           [IMPLEMENTED on MainPage page 1] [Humidity,Rain,Wind]
        ▪ Real-Time weather from OpenWeatherMap or WebAPI/Services                            [IMPLEMENTED on MainPage page 1] [Real icons , temperature , weather info]

• Alarm
    o Add alarm based on Date and Time                                                        [IMPLEMENTED on MainPage page 2] [DatePicker when add alarm , TimePicker when add alarm]
    o Add alarm based on Day and Time                                                         [IMPLEMENTED on MainPage page 2] [Click the buttons M,T,W,T,F,S,S , TimePicker when add alarm]
    o List Alarm with enable/disable feature                                                  [IMPLEMENTED on MainPage page 2] [Enabled and Disabled click checkbox]

• 3-Day Weather Forecast                                                                      [IMPLEMENTED on MainPage page 1] [Real icons , temperature , weather info]
    o (Any 5 types weather information)                                                       [IMPLEMENTED on MainPage page 1] [Wind direction,Rain volume,Wind speed,Humidity,pressure]
        ▪ Real-time or Live information from WebAPI/Services                                  [IMPLEMENTED on MainPage page 1] [Real icons , temperature , weather info]

--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

Implement other Web Services (10 marks)
•Any 2 other web services other than Weather type
    o AI API                                                                                  [IMPLEMENTED on MainPage page 3] [AI's response is ~11s]
    o NEWS API                                                                                [IMPLEMENTED on self proposed feature page] //Button [Globe image] [Top right] on MainPage page 1
        o Sometimes the UrlToimage from the API is empty so when create button is clicked nothing happens
        o After adding 5-7 images it starts to desync and the text just follows which image you click 
        o Somehow canvas settop and setleft restricts the manipulation so I didn't mess with the UI part
    o Trivia API                                                                              [IMPLEMENTED on MainPage page 4] [Trivia questions to cure boredom]

--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

Others Enhancements (15 marks)
•Implement more features
    o News                                                                                     [IMPLEMENTED on self proposed feature page] //Button [Globe image] [Top right] on MainPage page 1
                                                                                                 
•Improve on existing features (Innovative/Creativity Elements)                                 [IMPLEMENTED on self proposed feature page] //Button [Globe image] [Top right] on MainPage page 1
    o Able to Delete news by dragging to bottom right and 
    o UNDOing the delete
    o Able to add infinite amount of news [UI created from C# not xaml]
    o Able to drag the news around in the canvas
    o Able to navigate with a sidebar

--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

AI Enhancements (5 marks)
Implement Chat or AI features                                                                  [IMPLEMENTED on MainPage page3] 
- Used RapidAPI's chatgpt4.0 
~ 11s response time