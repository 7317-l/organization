// ai.js

const express = require("express");
const router = express.Router();
const axios = require("axios");


// ================================
// AI 配置
// ================================

const API_KEY = "你的API_KEY";

const API_URL = "https://api.openai.com/v1/chat/completions";


// ================================
// 普通AI对话
// ================================

router.post("/chat", async (req, res) => {

    try {

        const {
            message
        } = req.body;


        const response = await axios.post(
            API_URL,
            {

                model:"gpt-4o-mini",

                messages:[

                    {
                        role:"system",
                        content:
                        `
                        你是一个专业的学习AI助手。

                        回答要求：
                        1. 简洁准确
                        2. 使用中文
                        3. 如果涉及学习问题，优先给出步骤化分析
                        `
                    },


                    {
                        role:"user",
                        content:message
                    }

                ],


                temperature:0.5

            },


            {

                headers:{
                    "Content-Type":"application/json",
                    "Authorization":
                    `Bearer ${API_KEY}`
                }

            }


        );


        res.json({

            success:true,

            answer:
            response.data.choices[0].message.content

        });



    }catch(error){

        console.log(error.response?.data || error);


        res.json({

            success:false,

            answer:"AI暂时无法回答，请稍后再试"

        });

    }


});





// ================================
// AI错题总结
// ================================


router.post("/summary", async(req,res)=>{


    try{


        const {

            wrongQuestions

        } = req.body;



        if(!wrongQuestions || wrongQuestions.length===0){


            return res.json({

                success:false,

                message:"暂无错题数据"

            });

        }



        /*
        
        将错题整理成文本
        
        */

        let questionText="";


        wrongQuestions.forEach((item,index)=>{


            questionText +=
`
第${index+1}题：

题目：
${item.question || ""}

我的答案：
${item.userAnswer || ""}

正确答案：
${item.correctAnswer || ""}

解析：
${item.analysis || ""}

--------------------

`;

        });





        const prompt =

`
你是一名专业的AI学习分析老师。

现在请根据学生最近的错题数据，
进行学习情况总结。

要求：

不要逐题重复解析。

不要输出无关内容。

必须按照下面格式回答：


【一、错题整体情况分析】

分析学生整体错误趋势，
例如：
错误集中在哪些类型，
正确率情况，
主要问题。


【二、高频易错知识点】

列出学生最容易错误的知识点。

格式：

1.
知识点：
错误表现：
原因：


2.
知识点：
错误表现：
原因：



【三、错误原因分析】

从以下角度分析：

1.知识理解不足

2.公式/方法使用错误

3.审题问题

4.计算问题


【四、当前学习薄弱点】

总结学生目前最需要加强的能力。


【五、下一阶段强化建议】

给出具体学习计划：

例如：

1.需要重新学习什么

2.每天练习什么

3.如何避免再次错误


回答要求：

- 中文输出
- 条理清晰
- 使用标题
- 不超过800字


下面是错题数据：

${questionText}

`;




        const response = await axios.post(


            API_URL,


            {


                model:"gpt-4o-mini",


                messages:[


                    {

                        role:"system",

                        content:
                        `
                        你负责分析学生错题，
                        你的目标是帮助学生找到薄弱点。
                        必须按照指定模板回答。
                        `

                    },


                    {

                        role:"user",

                        content:prompt

                    }


                ],


                temperature:0.3


            },


            {

                headers:{


                    "Content-Type":"application/json",

                    "Authorization":
                    `Bearer ${API_KEY}`

                }

            }


        );



        res.json({


            success:true,


            summary:
            response.data.choices[0].message.content


        });




    }catch(error){



        console.log(
            "AI总结错误:",
            error.response?.data || error
        );



        res.json({


            success:false,


            message:"AI暂时无法生成总结"


        });



    }


});




module.exports = router;