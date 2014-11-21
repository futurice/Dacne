win-client-dev-ref-architecture
===============================

Repository for documentation and implementation of a reference architecture for (Universal) Windows apps build by Futurice

-----------------------------

## Scope ##

This section describes the app context to which the architecture is supposed to match ideally to. It lists the mandatory and optional functional and non-functional requirements that the apps have. It goes beyond just listing the requirements by discussing them and possible solutions in depth.

### Functional ###
*The apps will..*

#### RF1: Let user navigate between pages #### 
Needs to handle backstack and passing state from page to page, ideally in a type-safe way. Backstack is not always simple chronological stack. For example when deep linking into a specific page, it might be necessary to make back navigation take the user to the applications main page. Also, some apps might in some cases use hierarchical back stack rather than chronological one. In some cases it might be required to be able to serialize the backstack and return to the same state later (tombstoning).

(Architectural solutions are being explored by Antti Ahvenlampi)

#### RF2: Show popups and dialogs ####
Dialogs and popups might be implemented using the platform UI or a custom one. They might be fullscreen or modular and might need to block interactions with the underlaying page. They might need to pass the user chosen answer back to the invoker. Additionally, 'global' UI elements, such as an appbar, might need to be hidden or modified when a popup is shown.

#### RF3: Support easy optimization for different screen resolutions and differences in aspect ratios ####
Optimizations should be optional and in case an optimization is not found, a fallback should be used.
##### RF3.1 Assets #####
##### RF3.2 Font sizes #####
##### RF3.3 Seperate pages and layouts for different device categories #####

#### RF4: Get data from a backend ####
  - R4.1: Multiple backends (with different data formats) might be used
  - R4.2: Most likely json/xml from a restful web service
  - R4.3: In most cases caching the data and falling back to the cached data when disconnected is required
    - R4.3.1: Clearing / expiring the cache needs to be supported as well
  - R4.4: The backend, it's APIs, or the format of the data returned might change. The architecture needs to be easily adaptable to the changes.
  - R4.5: The APIs or the format provided might not be ideal for the app's object model. In many cases just mapping the data into model classes is not ideal. The architecture needs to be able to map any kind of backend into the object model that is ideal for the app. (Architecture needs to support changes in APIs and app requirements)
  - R4.6: Support development against wip APIs and in scenarios when the API can not be accessed
  - R4.7: Support debugging against rare, but know API responses
  - Might have multiple consequent (interrelated?) requests ongoing
   - Requests priorization
  - Show a progress indicator to the user while the backend request and parsing of the data is in progress
   - Ideally a progress bar
 - Show error notifications for user and allow retrying individual requests
 - Supports easy mocking of response data and errors from unit tests
   
(Architectural solutions are being explored by Olli-Matti Saario)
   
#### RF5: Parse the data into model objects and setup references between them ####
  - In some cases it is worthwhile to parse from the stream while the data is still being downloaded
    - However, in some cases the response data might not allow starting to parse before it has been fully loaded (data order)
  - In some cases data in memory needs to be partially updated from the downloaded data. In these cases it is important that the updates get pushed to the UI.
  - In some cases it is worthwhile to aim for pushing the parsed objects (or their carried changes) into the UI ASAP, while the downloading and parsing might still be ongoing.
    - Needs to be able to recover if downloading response data can not be fully completed (load previous succesfull response)

(Architectural solutions are being explored by Jarno Montonen)

#### Implement deep analytics to track both user actions and bugs ####

#### Support frequent updates ####
Update experience should be as smooth as possible with automatic data and settings migrations when necessary
The apps can't require extensive testing periods before updates, but try to make it up with fast reaction times to defects that get into production. The app needs to gracefully handle unhandled exceptions from secondary features (such as ads, logging, notifications). However, it is important that these events are logged and sent to an analytics backend.
  - Logged errors and crashes need to be available to the developer asap.
  - StackTraces and other error data needs to be as accurate as possible. It should include information on how to reproduce the event in debuggable environment.
 
#### Will be released as MVP as soon as possible ####

#### Greatly evolve during their lifespan #### 


### Non-functional ###

- R1: Start up fast enough
- Present the user with top of the line visual quality and fluidity (constantly good framerate)
- Need to be distributable as commercial closed source (3rd party component licenses)
- Can be efficiently developed and maintained by varying number of developers with varying leves of expertise and experience.
- Support continuous integration (visual studio online?)
- Can be realistically distributed via a 4G connection
- Have a memory footprint less than ~180 MB
- Don't use excessive amounts of disk space (keep track of usage)
- Support rapid GUI tweaking iterations and fast build+deploy process to facilitate developer+designer 'pair programming'
- Support efficient unit (and integration) testing (black box? white box?) with mocking (only platform classes? all classes?). Property based testing? (generating data sets to test against)

### Optional requirements ###
*The apps might need to*

- R3: Implement different GUIs and navigation structures for different devices (WP, Win 8, (Xbox One, iOS and Android with Xamarin))
- Handle authentication
  - The requirements vary from just blocking pages from unauthenticated users to more fine grained access control to the data (possible already dowloaded into the app)
  - In some cases registration and login are handled by third parties, and third party implementations, such as facebook, web view navigated to a web page implemented by a third party etc..
  - In some cases login and registration UIs are implemented by the app, in which cases efficient form handling with validation, hints and such is important.
- Push new data or updates to the received data to the backend
  - In some cases it is required to be able to store the changes on the device between sessions (and handle possible 'merge' conflicts when syncing)
- (Code up to view models might be required to be able to run on iOS and Android with Xamarin)
- Be localized to multiple languages
- Want to conserve battery by bundling network requests (for background tasks?)
- Want to be compiled with .NET Native for better performance

### Out of scope ###
The following items are things on which delibirate compromises can be made in favor of the mandatory or optional requirements listed above. 

THE APPS ARE UNLIKELY TO:
- Have automated UI tests
- Require extremely low input or update latency throughout the app (game -like)
- Have a development team larger than three developers
- Be implemented as HTML-hybrid apps
- Be solely designed by the developer(s) themselves
- Be mission critical: Defects are unlikely to put anyone's health at risk

## The architecture ##
This section describes the ideal architecture for the scope defined above in natural language. It lists, describes, and arguments patterns, frameworks, and libraries that should ideally be used in todays bussiness and technical environment. It explains how each of the mentioned items help to fullfill the requirements and how they tie in and can be used together.

## Implementation ##
This section presents actual project and solution templates that can be used to help implementing an app that follows the architecture defined in the previous section. It gives practical how-to's, tips, and discusses possible issues and workarounds.
