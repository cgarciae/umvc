**uMVC** is an MVC framework for Unity3D. It's built on top of [Zenject](https://github.com/modesttree/Zenject) and inspired by this excellent [blog post](http://engineering.socialpoint.es/MVC-pattern-unity3d-ui.html) on how to implement the MVC pattern in Unity.

It provides convention for building application in Unity3D in a sane and organized way that mimics the way many web frameworks are structured. uMVC tackles problems like creating testable code, organizing big projects, creating stateless views, creating reusable components, the hierarchical prefab update issue. Most importantly, uMVC makes you more productive on big projects by letting you worry about your views/presenters and letting uMVC take care of setting up dependencies and instantiating your prefabs. 
uMVC also comes with `middleware` so you can handle e.g. authorization code independently. In many ways its similar to how Angular 2 handles MVC.

Its main feature is that you can create presenters easy by simply using annotation and inheriting from the `MVCPresenter` class. Example:

```
[Presenter(path)]
public class MyPresenter: MVCPresenter
```

and this automatically allows you to be able to navigate to that presenter by using the the router like this

```
router.GoTo
(
    url: "my-presenter"
);
```

Another cool feature is you can create reusable elements and use them in many parts of your application, much in the spirit of Web Components. This is done mainly by using dependency injection.

Checkout our [Getting Started](https://github.com/cgarciae/umvc/wiki/Getting-Started) tutorial to create your first MVC App in Unity3D.
