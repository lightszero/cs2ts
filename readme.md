����һ��C#������ֲTypeScript�õĸ������ߡ�

2016-03-04 �����ֿ�

����һֱ�]��ʵ���ϵĽ�չ����Ϊһ��ʼ�ǲ���c#light�ķ���������һ��c#�Ľ�����������Ҳ��һ����̫�ɹ�����Ŀ��

ֱ�� 2019 �� 8 ��

��΢�����ѧϰ��һ��rosyln����c#Դ������ⲿ�ֹ�����ñȽϼ��ˡ����C# -> TypeScript ��ֲ���ߵ��Լ���������

���ת�����߿���ʵ������csproj��Ŀ�ĸ�����ֲ


1.Ŀǰ�Ѿ�����˻����ķ�������,����Ĵ���ת�����������ԣ���Ȼ��û�и��ǲ���
2.��������������c# �� using ת��������������
3.loops ת���� if �� for, foreach while dowhile switch ��û��ת������Щ����ͬС�죬�ܿ�Ϳ��Բ�ȫ
4.���������ֵ�ת�� Console.WriteLine -> console.log  string.Length => string.length

��һ��
1.ʵ������loops���ʽ
2.�õ�Ԫ���Ը���
3.�Բ���ʵ�ֵĹ��ܽ��о�������
4.����ʽ��ʹ�ý���
5.����һ��API����,����ʹ��c# ���붨��һЩAPI


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


