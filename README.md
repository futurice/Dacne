Mobile Apps Reference Architecture
===============================

Repository for documenting a reference architecture for mobile apps build by Futurice.

-----------------------------

## Scope

This section describes the app context to which the architecture is designed for. It lists the mandatory and optional functional and non-functional requirements that the apps have. It goes beyond just listing the requirements by discussing them and the possible architectural solutions in depth.

### Functional
This section describes the functional requirements and the possible architectural solutions for each of them. Each solution discusses its suitability to the functional requirement itself, and to all of the non-functional requirements. Some architectural solutions might be offered as a possible solution for multiple functional requirements. However, in these cases, they should be discussed from different angles in different requirements sections.

#### Let user navigate between pages
Needs to handle backstack and passing state from page to page, ideally in a type-safe way. Backstack is not always simple chronological stack. For example when deep linking into a specific page, it might be necessary to make back navigation take the user to the applications main page. Also, some apps might in some cases use hierarchical back stack rather than chronological one. In some cases it might be required to be able to serialize the backstack and return to the same state later (tombstoning).

#### Get data from a backend
  - Multiple backends (with different data formats) might be used
  - Most likely json/xml from a restful web service
  - In most cases caching the data and falling back to the cached data when disconnected is required
    - Clearing / expiring the cache needs to be supported as well
  - The backend, it's APIs, or the format of the data returned might change. The architecture needs to be easily adaptable to the changes.
  - The APIs or the format provided might not be ideal for the app's object model. In many cases just mapping the data into model classes is not ideal. The architecture needs to be able to map any kind of backend into the object model that is ideal for the app. (Architecture needs to support changes in APIs and app requirements)
  - Support development against wip APIs and in scenarios when the API can not be accessed
  - Support debugging against rare, but know API responses
  - Might have multiple consequent (interrelated?) requests ongoing
   - Requests priorization
  - Show a progress indicator to the user while the backend request and parsing of the data is in progress
   - Ideally a progress bar
 - Show error notifications for user and allow retrying individual requests
 - Supports easy mocking of response data and errors from unit tests
 - Might want to conserve battery by bundling network requests (for background tasks?)

#### Parse the data into model objects and setup references between them
  - In some cases it is worthwhile to parse from the stream while the data is still being downloaded
    - However, in some cases the response data might not allow starting to parse before it has been fully loaded (data order)
  - In some cases data in memory needs to be partially updated from the downloaded data. In these cases it is important that the updates get pushed to the UI.
  - In some cases it is worthwhile to aim for pushing the parsed objects (or their carried changes) into the UI ASAP, while the downloading and parsing might still be ongoing.
    - Needs to be able to recover if downloading response data can not be fully completed (load previous succesfull response)

#### Present models to the user and let the user interact with them
Models and hierarchies of models need to be presented to the user in vastly different UIs. User will need to be able to interact with the models and in some cases modify them or show a particular model in a different view. Additionally, there might be a need to present somewhat different models in the same views or present the same model in different views. Views should be able to adapt to varying amounts of data in a model, eg. if a property is missing, a collection has zero, one, or thousand items. The views should also be able to react to changes in model data or hierarchy somewhat instantly. UI should not block while models are being loaded and constructed, but indicate the situation to the user.

#### Show popups and dialogs
Dialogs and popups might be implemented using the platform UI or a custom one. They might be fullscreen or modular and might need to block interactions with the underlaying page. They might need to pass user's response back to the invoker. Additionally, 'global' UI elements, such as an appbar, might need to be hidden or modified when a popup is shown.

#### Support easy optimization for different screen resolutions and differences in aspect ratios
Optimizations should be optional and in case an optimization is not found, a fallback should be used.
##### Assets
##### Font sizes
##### Seperate pages and layouts for different device categories

#### Implement deep analytics to track user's and the app's behavior

#### Support frequent updates
Update experience should be as smooth as possible with automatic data and settings migrations when necessary.
The apps can't require extensive testing periods before updates, but try to make it up with fast reaction times to defects that slip into production. The app needs to gracefully handle unhandled exceptions from secondary features (such as ads, logging, and notifications). However, it is important that these events are logged and sent to an analytics backend.
  - Logged errors and crashes need to be available to the developer asap.
  - StackTraces and other error data needs to be as accurate as possible. It should include information on how to reproduce the event in debuggable environment.

#### (optional) Implement different GUIs and navigation structures for different devices (phones, desktops, tablets, living room devices, head-mounted devices..)

#### (optional) Handle authentication

- The requirements vary from just blocking pages from unauthenticated users to more fine grained access control to the data (possible already dowloaded into the app)
- In some cases registration and login are handled by third parties, and third party implementations, such as facebook, web view navigated to a web page implemented by a third party etc..
- In some cases login and registration UIs are implemented by the app, in which cases efficient form handling with validation, hints and such is important.

#### (optional) Push new data or updates to the received data to the backend

- In some cases it is required to be able to store the changes on the device between sessions (and handle possible 'merge' conflicts when syncing)

#### (optional) Be localized to multiple languages

### Non-functional

- Start up fast enough
- Present the user with top of the line visual quality and fluidity (constantly good framerate)
- Need to be distributable as commercial closed source (3rd party component licenses)
- Can be efficiently developed and maintained by varying number of developers with varying leves of expertise and experience.
- Support continuous integration
- Can be realistically distributed via a 4G connection
- Have a memory footprint less than ~180 MB
- Don't use excessive amounts of disk space (keep track of usage)
- Support rapid GUI tweaking iterations and fast build+deploy process to facilitate developer+designer 'pair programming'
- Support efficient unit (and integration) testing (black box? white box?) with mocking (only platform classes? all classes?). Property based testing? (generating data sets to test against)
- Will be released as MVP as soon as possible
- Greatly evolve during their lifespan
- (optional) Code up to view models might be required to be able to run on iOS and Android with Xamarin
- (optional) Want to be compiled with .NET Native for better performance

### Out of scope
The following items are things on which delibirate compromises can be made in favor of the mandatory or optional requirements listed above. 

THE APPS ARE UNLIKELY TO:
- Have automated UI tests
- Require extremely low input or update latency throughout the app (game -like)
- Have a development team larger than three developers
- Be implemented as HTML-hybrid apps
- Be solely designed by the developer(s) themselves
- Be mission critical: Defects are unlikely to put anyone's health at risk

## The architecture
This section describes the ideal architecture for the scope defined above in natural language. It lists, describes, and arguments patterns, frameworks, and libraries that should ideally be used in todays bussiness and technical environment. It explains how each of the mentioned items help to fulfill the requirements and how they tie in and can be used together.

### Data Access Layer

#### Diagrams (wip)

1. https://github.com/futurice/mobile-apps-reference-architecture/blob/master/diagrams/highlevel_wip_1.jpg
2. https://github.com/futurice/mobile-apps-reference-architecture/blob/master/diagrams/highlevel_wip_2.jpg
3. https://github.com/futurice/mobile-apps-reference-architecture/blob/master/diagrams/highlevel_wip_3.jpg

#### ILoadable<IdentifierType>

Each model that should be loaded needs to implement the interface.
- get IdentifierType Identifier
- get bool ShouldBeCached
- get bool ShouldBeLoadedFromCache
- set bool IsLoading
- (set bool IsLoaded)
- (set bool IsCached)
- set LoadError LoadError

There can ofc be a default LoadableModelBase : ILoadable<string>..

Hmm.. or should the Identifier by just IComparable and implement ToString() in a meaningful way.. Probably not, as the ModelLoader might need specific id info on ILoadables to be able to create the right request and choose the right parser. However, custom Identifiers in ILoadable wouldn't be needed if it know about the model types.. 

#### Error Handling

Error handling in the Data Access layer is handled via the LoadError property on the ILoadable interface. The different components (Parser, CachedRequest, ModelLoader) can set an instance of LoadError created by them into the property. If there's already an error in the model instance, the existing error should be set as the InnerError of the new error. 

For data load operation retrying LoadError class has an Retry delegate that can be set when the instance is created. In most common case, for example, ModelRepository would notice that model loading operation ended and an error was set to the model instance (by some of the components in the load chain), and would replace that error with a new error that has the old error as it's inner error, and the ModelRepository's initial load operation as the retry delegate. 

#### Parsers

Parsers are implemented for each data API and 'set' to the data access layer's (custom)ModelLoader. They use Model Repository's methods to get (create new) model instances when necessary and set their properties.

#### Caching

The ILoadable interface exposes ShouldBeCached and ShouldBeLoaded from cache getters. Cached request uses these getters to determine whether a request should be cached or not, as well as whether the request should be loaded from cache (if exists) or not.

### Universal Windows Apps

#### Working with Immutable Models and Mutable View Models

A proven model to work with data from backends, is to make all model objects immutable. In this model, the caching logic and can be easily encapsulated into one central location, from which all other parts of the application can retrieve the latest version of data. This cache mechanism should be designed in such a way, that its clients can always request data from it, no matter what the current state of the cache is. This means that one cache request might trigger several backend requests. The state of the model layer is thus hidden behind the model cache, and is visible only as a variable delay in fetching the data, or errors when loading data fails.

In this model all explicit mutable application state should be kept in the View and ViewModel. View Models have access to the cache layer, and present their current state through properties which are bound to the UI via Data Binding. Possible communications in the direction of the backend should also be mediated by the View Model, presenting the state of the transaction to the user when necessary. Alternatively, such communication can happen in the View, depending on the nature of the request: this is a case where strictly abiding to the MVVM pattern can introduce unnecessary code bloat - don't be a slave to the pattern. 

Since the View Models carry state in this model, we need to consider when they need to be invalidated. This is usually only an issue in back-navigation, when returning to a page that has been previously open. There are several ways to do this, and since navigation is a view-level action, it is sometimes simplest to implement it in the View, but could also be handled purely on a View Model level.

If there is some piece of data that is constantly updating, using Rx is a good choice. Set up an IObservable in the model, and expose e.g. a ReactiveProperty hooked up to that in the View Model. The cache layer only needs to care about updating the Observable, and the View Model doesn't need to care about how or when the data is updated.

## Implementation
This section presents actual platform specific project and solution templates that can be used to help implementing an app that follows the architecture defined in the previous section. It gives practical how-to's, tips, and discusses possible issues and workarounds.

### .NET Frameworks

- Caliburn.Micro: "Magic Driven Development"
- MVVMLight: Not the most active anymore
- MVVMCross: Mature and does the job
- ReactiveUI: The new guy integrating with reactive extensions
- Prism: Started at Microsoft as a way to build modular software. Recently moved into OS community and started to bring in MVVM specific stuff.
- Catel: A massive but modular end-to-end MVVM (and even ASP.NET MVC). Actively developed, but only by a one guy.
