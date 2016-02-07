**uMVC** is an MVC framework for Unity3D. It's built on top of [Zenject](https://github.com/modesttree/Zenject) and inspired by this excellent [blog post](http://engineering.socialpoint.es/MVC-pattern-unity3d-ui.html) on how to implement the MVC pattern in Unity.

Its main feature is that you can create presenters easy by simply using annotation and inheriting from the `MVCPresenter` class. Example:

```
[Presenter(path)]
public class MyPresenter: MVCPresenter
```

and this allows you to automatically be able to navigate your to that presenter by using the the router like this

```
router.GoTo
(
    url: "my-presenter"
);
```

Another cool feature is you can create reusable elements and use them in many parts of your application, much in the spirit of Web Components. This is done mainly by using dependency injection.

Checkout our [Getting Started](https://github.com/cgarciae/umvc/wiki/Getting-Started) tutorial to create your first MVC App in Unity3D.
