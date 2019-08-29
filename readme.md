这是一个C#程序移植TypeScript用的辅助工具。

2016-03-04 建立仓库

可是一直沒有实质上的进展，因为一开始是采用c#light的方案，这是一个c#的解释器，本身也是一个不太成功的项目。

直到 2019 年 8 月

稍微深入的学习了一下rosyln，在c#源码解析这部分工作变得比较简单了。这个C# -> TypeScript 移植工具得以继续开发。

这个转换工具可以实现整个csproj项目的辅助移植


1.目前已经完成了基本的翻译引擎,常规的代码转换基本都可以，当然还没有覆盖测试
2.可以正常的清理c# 的 using 转换成完整的名称
3.loops 转换了 if 和 for, foreach while dowhile switch 还没有转换，这些都大同小异，很快就可以补全
4.可以做名字的转换 Console.WriteLine -> console.log  string.Length => string.length

下一步
1.实现所有loops表达式
2.用单元测试覆盖
3.对不可实现的功能进行警告提醒
4.更正式的使用界面
5.设置一个API机制,可以使用c# 代码定义一些API


`using System;
`using System.Collections.Generic;
`using System.Text;
`
`namespace helloha
`{
`    public class ShowMTable
`    {
`        public void Show()
`        {
`            for (var i = 0; i < 9; i++)
`            {
`                string line = "";
`                for (var j = 0; j < 9; j++)
`                {
`                    var result = (i * j).ToString();
`                    if (result.Length == 1)
`                        result += "  ";
`                    else
`                        result += " ";
`                    line += (i + "*" + j + "=" + result);
`                }
`                line += "\n";
`                Console.WriteLine(line);
`            }
`        }
`    }
`}

==>

`namespace helloha
`{
`    export class ShowMTable
`    {
`        Show(): void
`        {
`            for (let i: number = 0; i < 9; i++)
`            {
`                let line: string = "";
`                for (let j: number = 0; j < 9; j++)
`                {
`                    let result: string = (i * j).toString();
`                    if (result.length == 1)
`                        result += "  ";
`                    else
`                        result += " ";
`                    line += (i + "*" + j + "=" + result);
`                }
`                line += "\n";
`                console.log(line);
`            }
`        }
`    }
`}


