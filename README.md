使用说明：

编写脚本→按下F5进行编译（右侧预览）→按下F4导出→在动作右键菜单中添加手势

每个函数都有对应的说明，补全时会看到。

![](https://files.getquicker.net/_sitefiles/kb/sharedaction/9279df73-3eed-4d30-f23c-08ddd4e0681e/2025/08/07/092116_679811_PixPin_2025-08-07_09-21-12.gif)

默认直角坐标系坐标系，也有其他模式可以选择（Windows坐标系——y轴向下)：

如果习惯[GestureHelper](https://getquicker.net/Sharedaction?code=ec1e99be-3699-4a11-74de-08dc9e8ef04d)的写法，可以选Windows

![](https://files.getquicker.net/_sitefiles/kb/sharedaction/9279df73-3eed-4d30-f23c-08ddd4e0681e/2025/08/07/092309_679811_PixPin_2025-08-07_09-22-49.png)

通常只会用到一些简单的计算（Math）和GestureDrawer提供的方法，支持链式调用，脚本最后只接受GestureDrawer类型。



更新说明：
v1→v2：

使用MathAngle进行“角度”计算与管理
重构Arc和DrawArc（已private），暴露更易用的DrawArc和DrawArcRelative（自动寻找圆心）
从v1迁移到v2主要就是要注意这里：函数名均改为DrawArc，删去前两个参数（横纵坐标）
移除弧度制
UI细节：编辑器宽度放大，手势预览增加边距，使图形更集中
