Read "D:\刘晨圆大二\服务外包\ai-module\frontend\src\views\AIChat.vue":

     1	<template>
     2	  <div class="ai-chat-overlay">
     3	
     4	    <!-- ==========================================
     5	         AI 主面板
     6	         ========================================== -->
     7	
     8	    <div class="ai-chat">
     9	
    10	      <!-- ========================================
    11	           顶部
    12	           ======================================== -->
    13	
    14	      <header class="ai-header">
    15	
    16	        <div class="ai-header-left">
    17	
    18	          <div class="ai-logo">
    19	            <span class="ai-logo-core">AI</span>
    20	          </div>
    21	
    22	          <div class="ai-header-title">
    23	
    24	            <div class="ai-title">
    25	              AI 学习助手
    26	            </div>
    27	
    28	            <div class="ai-subtitle">
    29	              智能分析 · 错题辅导 · 学习诊断
    30	            </div>
    31	
    32	          </div>
    33	
    34	        </div>
    35	
    36	
    37	        <div class="ai-header-right">
    38	
    39	          <div class="ai-status">
    40	            <span class="ai-status-dot"></span>
    41	            AI 在线
    42	          </div>
    43	
    44	          <button
    45	            class="close-button"
    46	            @click="closeAI"
    47	          >
    48	            ×
    49	          </button>
    50	
    51	        </div>
    52	
    53	      </header>
    54	
    55	
    56	      <!-- ========================================
    57	           主体
    58	           ======================================== -->
    59	
    60	      <div class="ai-body">
    61	
    62	        <!-- ======================================
    63	             左侧导航
    64	             ====================================== -->
    65	
    66	        <aside class="ai-sidebar">
    67	
    68	          <button
    69	            class="sidebar-item"
    70	            :class="{
    71	              active:
    72	                activePanel === 'chat'
    73	            }"
    74	            @click="
    75	              activePanel = 'chat'
    76	            "
    77	          >
    78	
    79	            <span class="sidebar-icon">
    80	              💬
    81	            </span>
    82	
    83	            <span>
    84	              普通AI对话
    85	            </span>
    86	
    87	          </button>
    88	
    89	
    90	          <button
    91	            class="sidebar-item"
    92	            :class="{
    93	              active:
    94	                activePanel === 'analysis'
    95	            }"
    96	            @click="
    97	              activePanel = 'analysis'
    98	            "
    99	          >
   100	
   101	            <span class="sidebar-icon">
   102	              ✦
   103	            </span>
   104	
   105	            <span>
   106	              AI分析
   107	            </span>
   108	
   109	          </button>
   110	
   111	
   112	          <button
   113	            class="sidebar-item"
   114	            :class="{
   115	              active:
   116	                activePanel === 'summary'
   117	            }"
   118	            @click="
   119	              activePanel = 'summary'
   120	            "
   121	          >
   122	
   123	            <span class="sidebar-icon">
   124	              📈
   125	            </span>
   126	
   127	            <span>
   128	              AI学习总结
   129	            </span>
   130	
   131	          </button>
   132	
   133	
   134	          <button
   135	            class="sidebar-item"
   136	            :class="{
   137	              active:
   138	                activePanel === 'wrong'
   139	            }"
   140	            @click="
   141	              activePanel = 'wrong'
   142	            "
   143	          >
   144	
   145	            <span class="sidebar-icon">
   146	              ❌
   147	            </span>
   148	
   149	            <span>
   150	              错题本
   151	            </span>
   152	
   153	            <span
   154	              v-if="wrongAnswers.length > 0"
   155	              class="sidebar-count"
   156	            >
   157	              {{ wrongAnswers.length }}
   158	            </span>
   159	
   160	          </button>
   161	
   162	
   163	          <button
   164	            class="sidebar-item"
   165	            :class="{
   166	              active:
   167	                activePanel === 'study'
   168	            }"
   169	            @click="
   170	              activePanel = 'study'
   171	            "
   172	          >
   173	
   174	            <span class="sidebar-icon">
   175	              📊
   176	            </span>
   177	
   178	            <span>
   179	              学习情况
   180	            </span>
   181	
   182	          </button>
   183	
   184	
   185	          <div class="sidebar-divider"></div>
   186	
   187	
   188	          <button
   189	            class="sidebar-item"
   190	            :class="{
   191	              active:
   192	                activePanel === 'history'
   193	            }"
   194	            @click="
   195	              activePanel = 'history'
   196	            "
   197	          >
   198	
   199	            <span class="sidebar-icon">
   200	              📝
   201	            </span>
   202	
   203	            <span>
   204	              答题记录
   205	            </span>
   206	
   207	          </button>
   208	
   209	
   210	          <!-- ====================================
   211	               当前识别题目
   212	               ==================================== -->
   213	
   214	          <div
   215	            v-if="hasQuestion"
   216	            class="sidebar-question-card"
   217	          >
   218	
   219	            <div class="sidebar-question-label">
   220	              当前题目
   221	            </div>
   222	
   223	            <div class="sidebar-question-text">
   224	              {{ questionData.question }}
   225	            </div>
   226	
   227	          </div>
   228	
   229	        </aside>
   230	
   231	
   232	        <!-- ======================================
   233	             右侧内容
   234	             ====================================== -->
   235	
   236	        <main class="ai-content">
   237	
   238	          <!-- ====================================
   239	               AI分析
   240	               ==================================== -->
   241	
   242	          <section
   243	            v-if="activePanel === 'chat'"
   244	            class="panel chat-panel"
   245	          >
   246	
   247	            <div class="panel-header">
   248	
   249	              <div>
   250	
   251	                <h2>
   252	                  普通AI对话
   253	                </h2>
   254	
   255	                <p>
   256	                  可以像使用普通智能助手一样自由提问
   257	                </p>
   258	
   259	              </div>
   260	
   261	            </div>
   262	
   263	            <div class="chat-messages normal-chat-messages">
   264	
   265	              <div
   266	                v-for="(message, index) in chatMessages"
   267	                :key="index"
   268	                class="chat-message"
   269	                :class="message.role"
   270	              >
   271	
   272	                <div
   273	                  v-if="message.role === 'assistant'"
   274	                  class="chat-avatar"
   275	                >
   276	                  AI
   277	                </div>
   278	
   279	                <div class="chat-bubble">
   280	                  {{ message.content }}
   281	                </div>
   282	
   283	              </div>
   284	
   285	            </div>
   286	
   287	            <div class="chat-input-area">
   288	
   289	              <input
   290	                v-model="chatInput"
   291	                type="text"
   292	                placeholder="请输入你想咨询的问题"
   293	                @keyup.enter="sendMessage"
   294	              />
   295	
   296	              <button
   297	                class="send-button"
   298	                :disabled="!chatInput.trim()"
   299	                @click="sendMessage"
   300	              >
   301	                发送
   302	              </button>
   303	
   304	            </div>
   305	
   306	          </section>
   307	
   308	
   309	          <section
   310	            v-if="activePanel === 'analysis'"
   311	            class="panel analysis-panel"
   312	          >
   313	
   314	            <div class="panel-header">
   315	
   316	              <div>
   317	
   318	                <h2>
   319	                  AI智能分析
   320	                </h2>
   321	
   322	                <p>
   323	                  AI仅识别当前正在作答的题目，并判断该题是否需要分析
   324	                </p>
   325	
   326	              </div>
   327	
   328	
   329	              <button
   330	                class="analyze-button"
   331	                :disabled="
   332	                  analysisLoading ||
   333	                  !hasQuestion
   334	                "
   335	                @click="startAnalysis"
   336	              >
   337	
   338	                <span
   339	                  v-if="analysisLoading"
   340	                  class="loading-spinner"
   341	                ></span>
   342	
   343	                <span v-if="analysisLoading">
   344	                  正在分析...
   345	                </span>
   346	
   347	                <span v-else>
   348	                  ✦ 分析当前题目
   349	                </span>
   350	
   351	              </button>
   352	
   353	            </div>
   354	
   355	
   356	            <!-- ==================================
   357	                 自动错题提示
   358	                 ================================== -->
   359	
   360	            <div
   361	              v-if="
   362	                autoHelp &&
   363	                autoHelp.question
   364	              "
   365	              class="auto-help-banner"
   366	            >
   367	
   368	              <div class="auto-help-banner-icon">
   369	                💡
   370	              </div>
   371	
   372	              <div class="auto-help-banner-content">
   373	
   374	                <div class="auto-help-banner-title">
   375	                  AI检测到这是一道错题
   376	                </div>
   377	
   378	                <div class="auto-help-banner-text">
   379	                  已自动为你准备本题解析，帮助你理解错误原因。
   380	                </div>
   381	
   382	              </div>
   383	
   384	            </div>
   385	
   386	
   387	            <!-- ==================================
   388	                 没有题目
   389	                 ================================== -->
   390	
   391	            <div
   392	              v-if="!hasQuestion"
   393	              class="empty-state"
   394	            >
   395	
   396	              <div class="empty-icon">
   397	                ✦
   398	              </div>
   399	
   400	              <div class="empty-title">
   401	                暂无可分析题目
   402	              </div>
   403	
   404	              <div class="empty-text">
   405	                请先完成一道题目，AI将根据你的答题情况进行分析。
   406	              </div>
   407	
   408	            </div>
   409	
   410	
   411	            <!-- ==================================
   412	                 题目卡片
   413	                 ================================== -->
   414	
   415	            <div
   416	              v-if="hasQuestion"
   417	              class="question-analysis-card"
   418	            >
   419	
   420	              <div class="question-card-top">
   421	
   422	                <div class="question-number">
   423	                  第 {{ questionNumber }} 题
   424	                </div>
   425	
   426	                <div
   427	                  v-if="questionData.submitted"
   428	                  class="answer-status"
   429	                  :class="{
   430	                    correct:
   431	                      questionData.correct,
   432	                    wrong:
   433	                      !questionData.correct
   434	                  }"
   435	                >
   436	
   437	                  <span
   438	                    v-if="questionData.correct"
   439	                  >
   440	                    ✓ 回答正确
   441	                  </span>
   442	
   443	                  <span
   444	                    v-else
   445	                  >
   446	                    ✕ 回答错误
   447	                  </span>
   448	
   449	                </div>
   450	
   451	              </div>
   452	
   453	
   454	              <div class="question-title">
   455	                {{ questionData.question }}
   456	              </div>
   457	
   458	
   459	              <!-- =================================
   460	                   选项
   461	                   ================================= -->
   462	
   463	              <div class="question-options">
   464	
   465	                <div
   466	                  v-for="(option, index) in normalizedOptions"
   467	                  :key="option.key || index"
   468	                  class="question-option"
   469	                  :class="{
   470	                    selected:
   471	                      isSelectedOption(option),
   472	                    correct:
   473	                      questionData.submitted &&
   474	                      isCorrectOption(option),
   475	                    wrong:
   476	                      questionData.submitted &&
   477	                      isSelectedOption(option) &&
   478	                      !isCorrectOption(option)
   479	                  }"
   480	                >
   481	
   482	                  <span class="option-letter">
   483	                    {{ option.key }}
   484	                  </span>
   485	
   486	                  <span class="option-text">
   487	                    {{ option.text }}
   488	                  </span>
   489	
   490	                  <span
   491	                    v-if="
   492	                      questionData.submitted &&
   493	                      isCorrectOption(option)
   494	                    "
   495	                    class="option-result"
   496	                  >
   497	                    ✓
   498	                  </span>
   499	
   500	                  <span
   501	                    v-else-if="
   502	                      questionData.submitted &&
   503	                      isSelectedOption(option) &&
   504	                      !isCorrectOption(option)
   505	                    "
   506	                    class="option-result wrong-icon"
   507	                  >
   508	                    ×
   509	                  </span>
   510	
   511	                </div>
   512	
   513	              </div>
   514	
   515	            </div>
   516	
   517	
   518	            <!-- ==================================
   519	                 AI分析内容
   520	                 ================================== -->
   521	
   522	            <div
   523	              v-if="
   524	                hasQuestion &&
   525	                analysisResult
   526	              "
   527	              class="analysis-result"
   528	            >
   529	
   530	              <div class="analysis-result-header">
   531	
   532	                <div class="analysis-ai-avatar">
   533	                  AI
   534	                </div>
   535	
   536	                <div>
   537	
   538	                  <div class="analysis-ai-name">
   539	                    AI 学习助手
   540	                  </div>
   541	
   542	                  <div class="analysis-ai-tip">
   543	                    针对本题生成的学习分析
   544	                  </div>
   545	
   546	                </div>
   547	
   548	              </div>
   549	
   550	
   551	              <!-- 正误判断 -->
   552	
   553	              <div class="analysis-section">
   554	
   555	                <div class="analysis-section-title">
   556	                  <span class="section-icon">
   557	                    🎯
   558	                  </span>
   559	
   560	                  答题判断
   561	                </div>
   562	
   563	                <div class="analysis-section-content">
   564	                  {{ analysisResult.judgement }}
   565	                </div>
   566	
   567	              </div>
   568	
   569	
   570	              <!-- 正确答案 -->
   571	
   572	              <div class="analysis-section">
   573	
   574	                <div class="analysis-section-title">
   575	                  <span class="section-icon">
   576	                    ✓
   577	                  </span>
   578	
   579	                  正确答案
   580	                </div>
   581	
   582	                <div class="answer-box">
   583	
   584	                  <span class="answer-label">
   585	                    {{ questionData.correctAnswer }}
   586	                  </span>
   587	
   588	                  <span>
   589	                    {{
   590	                      correctOptionText
   591	                    }}
   592	                  </span>
   593	
   594	                </div>
   595	
   596	              </div>
   597	
   598	
   599	              <!-- 知识点 -->
   600	
   601	              <div class="analysis-section">
   602	
   603	                <div class="analysis-section-title">
   604	                  <span class="section-icon">
   605	                    📚
   606	                  </span>
   607	
   608	                  核心知识点
   609	                </div>
   610	
   611	                <div class="knowledge-box">
   612	
   613	                  <div class="knowledge-name">
   614	                    {{
   615	                      questionData.knowledgePoint ||
   616	                      "本题核心知识点"
   617	                    }}
   618	                  </div>
   619	
   620	                  <div class="knowledge-description">
   621	                    {{
   622	                      analysisResult.knowledge
   623	                    }}
   624	                  </div>
   625	
   626	                </div>
   627	
   628	              </div>
   629	
   630	
   631	              <!-- 错误原因 -->
   632	
   633	              <div
   634	                v-if="
   635	                  !questionData.correct
   636	                "
   637	                class="analysis-section"
   638	              >
   639	
   640	                <div class="analysis-section-title">
   641	                  <span class="section-icon">
   642	                    ⚠
   643	                  </span>
   644	
   645	                  错误原因
   646	                </div>
   647	
   648	                <div class="mistake-box">
   649	
   650	                  <div class="mistake-title">
   651	                    为什么容易做错？
   652	                  </div>
   653	
   654	                  <div class="mistake-content">
   655	                    {{
   656	                      analysisResult.mistake
   657	                    }}
   658	                  </div>
   659	
   660	                </div>
   661	
   662	              </div>
   663	
   664	
   665	              <!-- 解题思路 -->
   666	
   667	              <div class="analysis-section">
   668	
   669	                <div class="analysis-section-title">
   670	                  <span class="section-icon">
   671	                    💡
   672	                  </span>
   673	
   674	                  解题思路
   675	                </div>
   676	
   677	                <div class="steps">
   678	
   679	                  <div
   680	                    v-for="
   681	                      (step, index)
   682	                      in analysisResult.steps
   683	                    "
   684	                    :key="index"
   685	                    class="step-item"
   686	                  >
   687	
   688	                    <div class="step-number">
   689	                      {{ index + 1 }}
   690	                    </div>
   691	
   692	                    <div class="step-content">
   693	                      {{ step }}
   694	                    </div>
   695	
   696	                  </div>
   697	
   698	                </div>
   699	
   700	              </div>
   701	
   702	
   703	              <!-- 学习建议 -->
   704	
   705	              <div class="analysis-section">
   706	
   707	                <div class="analysis-section-title">
   708	                  <span class="section-icon">
   709	                    🚀
   710	                  </span>
   711	
   712	                  学习建议
   713	                </div>
   714	
   715	                <div class="suggestion-box">
   716	                  {{
   717	                    analysisResult.suggestion
   718	                  }}
   719	                </div>
   720	
   721	              </div>
   722	
   723	            </div>
   724	
   725	
   726	            <!-- ==================================
   727	                 AI聊天区域
   728	                 ================================== -->
   729	
   730	            <div
   731	              v-if="hasQuestion"
   732	              class="chat-section"
   733	            >
   734	
   735	              <div class="chat-title">
   736	                还有疑问？继续问 AI
   737	              </div>
   738	
   739	
   740	              <div class="chat-messages">
   741	
   742	                <div
   743	                  v-for="
   744	                    (message, index)
   745	                    in chatMessages
   746	                  "
   747	                  :key="index"
   748	                  class="chat-message"
   749	                  :class="message.role"
   750	                >
   751	
   752	                  <div
   753	                    v-if="
   754	                      message.role === 'assistant'
   755	                    "
   756	                    class="chat-avatar"
   757	                  >
   758	                    AI
   759	                  </div>
   760	
   761	                  <div class="chat-bubble">
   762	                    {{ message.content }}
   763	                  </div>
   764	
   765	                </div>
   766	
   767	              </div>
   768	
   769	
   770	              <div class="chat-input-area">
   771	
   772	                <input
   773	                  v-model="chatInput"
   774	                  type="text"
   775	                  placeholder="例如：为什么这个选项是错的？"
   776	                  @keyup.enter="sendMessage"
   777	                />
   778	
   779	                <button
   780	                  class="send-button"
   781	                  :disabled="
   782	                    !chatInput.trim()
   783	                  "
   784	                  @click="sendMessage"
   785	                >
   786	                  发送
   787	                </button>
   788	
   789	              </div>
   790	
   791	            </div>
   792	
   793	          </section>
   794	
   795	
   796	          <!-- ====================================
   797	               错题本
   798	               ==================================== -->
   799	
   800	          <section
   801	            v-if="activePanel === 'summary'"
   802	            class="panel analysis-panel"
   803	          >
   804	
   805	            <div class="panel-header">
   806	
   807	              <div>
   808	                <h2>
   809	                  AI学习总结
   810	                </h2>
   811	
   812	                <p>
   813	                  根据你的错题数据分析薄弱知识点，并制定强化方案
   814	                </p>
   815	              </div>
   816	
   817	
   818	              <button
   819	                class="analyze-button"
   820	                :disabled="summaryLoading"
   821	                @click="startSummary"
   822	              >
   823	                <span v-if="summaryLoading">
   824	                  正在生成总结...
   825	                </span>
   826	
   827	                <span v-else>
   828	                  📊 生成学习总结
   829	                </span>
   830	
   831	              </button>
   832	
   833	            </div>
   834	
   835	
   836	            <div
   837	              v-if="summaryResult"
   838	              class="analysis-result"
   839	            >
   840	              {{ summaryResult }}
   841	            </div>
   842	
   843	
   844	            <div
   845	              v-else
   846	              class="empty-state"
   847	            >
   848	              点击按钮，让AI根据你的错题记录生成学习建议。
   849	            </div>
   850	
   851	          </section>
   852	
   853	
   854	          <section
   855	            v-if="activePanel === 'wrong'"
   856	            class="panel"
   857	          >
   858	
   859	            <div class="panel-header">
   860	
   861	              <div>
   862	
   863	                <h2>
   864	                  我的错题本
   865	                </h2>
   866	
   867	                <p>
   868	                  系统自动收集你答错过的题目
   869	                </p>
   870	
   871	              </div>
   872	
   873	              <div class="wrong-total">
   874	                共 {{ wrongAnswers.length }} 道错题
   875	              </div>
   876	
   877	            </div>
   878	
   879	
   880	            <!-- 没有错题 -->
   881	
   882	            <div
   883	              v-if="
   884	                wrongAnswers.length === 0
   885	              "
   886	              class="empty-state"
   887	            >
   888	
   889	              <div class="empty-icon success">
   890	                ✓
   891	              </div>
   892	
   893	              <div class="empty-title">
   894	                暂时没有错题
   895	              </div>
   896	
   897	              <div class="empty-text">
   898	                继续保持，你的答题表现很好！
   899	              </div>
   900	
   901	            </div>
   902	
   903	
   904	            <!-- 错题列表 -->
   905	
   906	            <div
   907	              v-else
   908	              class="wrong-list"
   909	            >
   910	
   911	              <div
   912	                v-for="
   913	                  (record, index)
   914	                  in wrongAnswers
   915	                "
   916	                :key="
   917	                  record.id ||
   918	                  index
   919	                "
   920	                class="wrong-item"
   921	                :class="{
   922	                  active:
   923	                    selectedWrongId ===
   924	                    (
   925	                      record.id ||
   926	                      index
   927	                    )
   928	                }"
   929	                @click="
   930	                  selectWrongAnswer(
   931	                    record
   932	                  )
   933	                "
   934	              >
   935	
   936	                <div class="wrong-item-number">
   937	                  {{ index + 1 }}
   938	                </div>
   939	
   940	                <div class="wrong-item-content">
   941	
   942	                  <div class="wrong-item-question">
   943	                    {{ record.question }}
   944	                  </div>
   945	
   946	                  <div class="wrong-item-meta">
   947	
   948	                    <span>
   949	                      你的答案：
   950	                      {{
   951	                        record.userAnswer ||
   952	                        "未作答"
   953	                      }}
   954	                    </span>
   955	
   956	                    <span>
   957	                      正确答案：
   958	                      {{
   959	                        record.correctAnswer
   960	                      }}
   961	                    </span>
   962	
   963	                  </div>
   964	
   965	                  <div
   966	                    v-if="
   967	                      record.knowledgePoint
   968	                    "
   969	                    class="wrong-item-knowledge"
   970	                  >
   971	                    {{ record.knowledgePoint }}
   972	                  </div>
   973	
   974	                </div>
   975	
   976	                <div class="wrong-arrow">
   977	                  →
   978	                </div>
   979	
   980	              </div>
   981	
   982	            </div>
   983	
   984	
   985	            <!-- 错题详情 -->
   986	
   987	            <div
   988	              v-if="selectedWrong"
   989	              class="wrong-detail"
   990	            >
   991	
   992	              <div class="wrong-detail-header">
   993	
   994	                <div>
   995	
   996	                  <div class="detail-label">
   997	                    错题解析
   998	                  </div>
   999	
  1000	                  <div class="detail-title">
  1001	                    {{ selectedWrong.question }}
  1002	                  </div>
  1003	
  1004	                </div>
  1005	
  1006	                <button
  1007	                  class="detail-close"
  1008	                  @click="
  1009	                    selectedWrongId = null
  1010	                  "
  1011	                >
  1012	                  ×
  1013	                </button>
  1014	
  1015	              </div>
  1016	
  1017	
  1018	              <div class="detail-answer-row">
  1019	
  1020	                <div class="detail-answer wrong-answer">
  1021	                  <span>
  1022	                    你的答案
  1023	                  </span>
  1024	
  1025	                  <strong>
  1026	                    {{
  1027	                      selectedWrong.userAnswer ||
  1028	                      "未作答"
  1029	                    }}
  1030	                  </strong>
  1031	                </div>
  1032	
  1033	
  1034	                <div class="detail-answer correct-answer">
  1035	                  <span>
  1036	                    正确答案
  1037	                  </span>
  1038	
  1039	                  <strong>
  1040	                    {{
  1041	                      selectedWrong.correctAnswer
  1042	                    }}
  1043	                  </strong>
  1044	                </div>
  1045	
  1046	              </div>
  1047	
  1048	
  1049	              <div
  1050	                v-if="
  1051	                  selectedWrong.knowledgePoint
  1052	                "
  1053	                class="detail-knowledge"
  1054	              >
  1055	
  1056	                <div class="detail-knowledge-title">
  1057	                  本题知识点
  1058	                </div>
  1059	
  1060	                <div>
  1061	                  {{
  1062	                    selectedWrong.knowledgePoint
  1063	                  }}
  1064	                </div>
  1065	
  1066	              </div>
  1067	
  1068	
  1069	              <button
  1070	                class="detail-analyze-button"
  1071	                @click="
  1072	                  analyzeWrongQuestion(
  1073	                    selectedWrong
  1074	                  )
  1075	                "
  1076	              >
  1077	                ✦ AI分析这道错题
  1078	              </button>
  1079	
  1080	            </div>
  1081	
  1082	          </section>
  1083	
  1084	
  1085	          <!-- ====================================
  1086	               学习情况
  1087	               ==================================== -->
  1088	
  1089	          <section
  1090	            v-if="activePanel === 'study'"
  1091	            class="panel"
  1092	          >
  1093	
  1094	            <div class="panel-header">
  1095	
  1096	              <div>
  1097	
  1098	                <h2>
  1099	                  学习情况
  1100	                </h2>
  1101	
  1102	                <p>
  1103	                  根据你的历史答题记录生成学习数据
  1104	                </p>
  1105	
  1106	              </div>
  1107	
  1108	            </div>
  1109	
  1110	
  1111	            <div class="statistics-grid">
  1112	
  1113	              <div class="stat-card">
  1114	
  1115	                <div class="stat-icon">
  1116	                  📝
  1117	                </div>
  1118	
  1119	                <div class="stat-value">
  1120	                  {{ totalAnswers }}
  1121	                </div>
  1122	
  1123	                <div class="stat-label">
  1124	                  总答题数
  1125	                </div>
  1126	
  1127	              </div>
  1128	
  1129	
  1130	              <div class="stat-card">
  1131	
  1132	                <div class="stat-icon">
  1133	                  ✓
  1134	                </div>
  1135	
  1136	                <div class="stat-value success-text">
  1137	                  {{ correctAnswers }}
  1138	                </div>
  1139	
  1140	                <div class="stat-label">
  1141	                  正确题数
  1142	                </div>
  1143	
  1144	              </div>
  1145	
  1146	
  1147	              <div class="stat-card">
  1148	
  1149	                <div class="stat-icon">
  1150	                  ✕
  1151	                </div>
  1152	
  1153	                <div class="stat-value danger-text">
  1154	                  {{ wrongAnswers.length }}
  1155	                </div>
  1156	
  1157	                <div class="stat-label">
  1158	                  错题数
  1159	                </div>
  1160	
  1161	              </div>
  1162	
  1163	
  1164	              <div class="stat-card">
  1165	
  1166	                <div class="stat-icon">
  1167	                  🎯
  1168	                </div>
  1169	
  1170	                <div class="stat-value">
  1171	                  {{ accuracy }}%
  1172	                </div>
  1173	
  1174	                <div class="stat-label">
  1175	                  正确率
  1176	                </div>
  1177	
  1178	              </div>
  1179	
  1180	            </div>
  1181	
  1182	
  1183	            <!-- 学习评价 -->
  1184	
  1185	            <div class="study-evaluation">
  1186	
  1187	              <div class="evaluation-title">
  1188	                AI学习评价
  1189	              </div>
  1190	
  1191	              <div class="evaluation-content">
  1192	                {{ studyEvaluation }}
  1193	              </div>
  1194	
  1195	            </div>
  1196	
  1197	
  1198	            <!-- 知识点统计 -->
  1199	
  1200	            <div class="knowledge-stat-card">
  1201	
  1202	              <div class="knowledge-stat-title">
  1203	                错题知识点分布
  1204	              </div>
  1205	
  1206	              <div
  1207	                v-if="
  1208	                  knowledgeStatistics.length === 0
  1209	                "
  1210	                class="knowledge-empty"
  1211	              >
  1212	                暂无足够数据
  1213	              </div>
  1214	
  1215	              <div
  1216	                v-else
  1217	                class="knowledge-list"
  1218	              >
  1219	
  1220	                <div
  1221	                  v-for="
  1222	                    item in knowledgeStatistics
  1223	                  "
  1224	                  :key="
  1225	                    item.name
  1226	                  "
  1227	                  class="knowledge-stat-item"
  1228	                >
  1229	
  1230	                  <div class="knowledge-stat-info">
  1231	
  1232	                    <span>
  1233	                      {{ item.name }}
  1234	                    </span>
  1235	
  1236	                    <span>
  1237	                      {{ item.count }} 题
  1238	                    </span>
  1239	
  1240	                  </div>
  1241	
  1242	                  <div class="progress-bar">
  1243	
  1244	                    <div
  1245	                      class="progress-bar-inner"
  1246	                      :style="{
  1247	                        width:
  1248	                          item.percent +
  1249	                          '%'
  1250	                      }"
  1251	                    ></div>
  1252	
  1253	                  </div>
  1254	
  1255	                </div>
  1256	
  1257	              </div>
  1258	
  1259	            </div>
  1260	
  1261	          </section>
  1262	
  1263	
  1264	          <!-- ====================================
  1265	               答题记录
  1266	               ==================================== -->
  1267	
  1268	          <section
  1269	            v-if="activePanel === 'history'"
  1270	            class="panel"
  1271	          >
  1272	
  1273	            <div class="panel-header">
  1274	
  1275	              <div>
  1276	
  1277	                <h2>
  1278	                  答题记录
  1279	                </h2>
  1280	
  1281	                <p>
  1282	                  查看你的历史答题情况
  1283	                </p>
  1284	
  1285	              </div>
  1286	
  1287	              <div class="history-count">
  1288	                {{ answerHistory.length }} 条记录
  1289	              </div>
  1290	
  1291	            </div>
  1292	
  1293	
  1294	            <div
  1295	              v-if="
  1296	                answerHistory.length === 0
  1297	              "
  1298	              class="empty-state"
  1299	            >
  1300	
  1301	              <div class="empty-icon">
  1302	                📝
  1303	              </div>
  1304	
  1305	              <div class="empty-title">
  1306	                暂无答题记录
  1307	              </div>
  1308	
  1309	              <div class="empty-text">
  1310	                完成题目后，这里会自动记录你的答题情况。
  1311	              </div>
  1312	
  1313	            </div>
  1314	
  1315	
  1316	            <div
  1317	              v-else
  1318	              class="history-list"
  1319	            >
  1320	
  1321	              <div
  1322	                v-for="
  1323	                  (record, index)
  1324	                  in reversedHistory
  1325	                "
  1326	                :key="
  1327	                  record.id ||
  1328	                  index
  1329	                "
  1330	                class="history-item"
  1331	              >
  1332	
  1333	                <div
  1334	                  class="history-status"
  1335	                  :class="{
  1336	                    correct:
  1337	                      record.correct,
  1338	                    wrong:
  1339	                      !record.correct
  1340	                  }"
  1341	                >
  1342	
  1343	                  <span
  1344	                    v-if="record.correct"
  1345	                  >
  1346	                    ✓
  1347	                  </span>
  1348	
  1349	                  <span v-else>
  1350	                    ×
  1351	                  </span>
  1352	
  1353	                </div>
  1354	
  1355	
  1356	                <div class="history-content">
  1357	
  1358	                  <div class="history-question">
  1359	                    {{ record.question }}
  1360	                  </div>
  1361	
  1362	                  <div class="history-meta">
  1363	
  1364	                    <span>
  1365	                      你的答案：
  1366	                      {{
  1367	                        record.userAnswer ||
  1368	                        "未作答"
  1369	                      }}
  1370	                    </span>
  1371	
  1372	                    <span>
  1373	                      正确答案：
  1374	                      {{
  1375	                        record.correctAnswer
  1376	                      }}
  1377	                    </span>
  1378	
  1379	                    <span
  1380	                      v-if="
  1381	                        record.knowledgePoint
  1382	                      "
  1383	                    >
  1384	                      {{ record.knowledgePoint }}
  1385	                    </span>
  1386	
  1387	                  </div>
  1388	
  1389	                </div>
  1390	
  1391	              </div>
  1392	
  1393	            </div>
  1394	
  1395	          </section>
  1396	
  1397	        </main>
  1398	
  1399	      </div>
  1400	
  1401	    </div>
  1402	
  1403	  </div>
  1404	</template>
  1405	
  1406	
  1407	<script setup>
  1408	
  1409	import {
  1410	  ref,
  1411	  computed,
  1412	  watch,
  1413	  onMounted
  1414	} from "vue"
  1415	
  1416	
  1417	/* =====================================================
  1418	   Props
  1419	   ===================================================== */
  1420	
  1421	const props = defineProps({
  1422	
  1423	  questionData: {
  1424	
  1425	    type: Object,
  1426	
  1427	    default: () => null
  1428	
  1429	  },
  1430	
  1431	
  1432	  answerHistory: {
  1433	
  1434	    type: Array,
  1435	
  1436	    default: () => []
  1437	
  1438	  },
  1439	
  1440	
  1441	  autoHelp: {
  1442	
  1443	    type: Object,
  1444	
  1445	    default: () => ({
  1446	
  1447	      visible:
  1448	        false,
  1449	
  1450	      question:
  1451	        null
  1452	
  1453	    })
  1454	
  1455	  }
  1456	
  1457	})
  1458	
  1459	
  1460	/* =====================================================
  1461	   Emits
  1462	   ===================================================== */
  1463	
  1464	const emit = defineEmits([
  1465	
  1466	  "close",
  1467	
  1468	  "clear-auto-help"
  1469	
  1470	])
  1471	
  1472	
  1473	/* =====================================================
  1474	   页面状态
  1475	   ===================================================== */
  1476	
  1477	const activePanel =
  1478	  ref("analysis")
  1479	
  1480	
  1481	const analysisLoading =
  1482	  ref(false)
  1483	
  1484	
  1485	const analysisResult =
  1486	  ref(null)
  1487	
  1488	
  1489	/* =====================================================
  1490	   AI学习总结
  1491	   ===================================================== */
  1492	
  1493	const summaryLoading =
  1494	  ref(false)
  1495	
  1496	
  1497	const summaryResult =
  1498	  ref("")
  1499	
  1500	
  1501	const startSummary =
  1502	  async () => {
  1503	
  1504	    if (
  1505	      !props.answerHistory ||
  1506	      props.answerHistory.length === 0
  1507	    ) {
  1508	
  1509	      summaryResult.value =
  1510	        "目前还没有足够的答题数据，请先完成一些题目。"
  1511	
  1512	      return
  1513	
  1514	    }
  1515	
  1516	
  1517	    summaryLoading.value = true
  1518	
  1519	
  1520	    try {
  1521	
  1522	      const wrongData =
  1523	        props.answerHistory.filter(
  1524	          item =>
  1525	            item.correct === false ||
  1526	            item.isCorrect === false ||
  1527	            item.correctAnswer !== item.userAnswer
  1528	        )
  1529	
  1530	
  1531	      const prompt = `
  1532	你是一名学习分析助手。
  1533	
  1534	请根据用户错题数据总结学习情况。
  1535	
  1536	要求：
  1537	1. 找出错误最多的知识点；
  1538	2. 判断用户薄弱方向；
  1539	3. 给出需要重点强化的内容；
  1540	4. 制定下一阶段学习计划。
  1541	
  1542	错题数据：
  1543	${JSON.stringify(wrongData)}
  1544	`
  1545	
  1546	
  1547	      const response =
  1548	        await fetch(
  1549	          "http://localhost:3000/api/ai/analyze-learning",
  1550	          {
  1551	            method:"POST",
  1552	            headers:{
  1553	              "Content-Type":"application/json"
  1554	            },
  1555	            body:JSON.stringify({
  1556	              totalQuestions: props.answerHistory.length,
  1557	              correctQuestions: props.answerHistory.filter(item => item.correct === true || item.isCorrect === true).length,
  1558	              wrongQuestions: wrongData.length,
  1559	              knowledgeStats: {},
  1560	              wrongQuestionsList: wrongData
  1561	            })
  1562	          }
  1563	        )
  1564	
  1565	
  1566	      const data =
  1567	        await response.json()
  1568	
  1569	
  1570	      console.log("AI学习总结返回数据：", data)
  1571	
  1572	      summaryResult.value =
  1573	        data?.data?.content ||
  1574	        data?.reply ||
  1575	        data?.message ||
  1576	        "AI暂时无法生成总结。"
  1577	
  1578	
  1579	    } catch(error) {
  1580	
  1581	      summaryResult.value =
  1582	        "生成学习总结失败，请稍后重试。"
  1583	
  1584	    } finally {
  1585	
  1586	      summaryLoading.value = false
  1587	
  1588	    }
  1589	
  1590	  }
  1591	
  1592	
  1593	const chatInput =
  1594	  ref("")
  1595	
  1596	
  1597	const chatMessages =
  1598	  ref(JSON.parse(localStorage.getItem("ai_chat_messages") || "[]"))
  1599	
  1600	
  1601	if (chatMessages.value.length === 0) {
  1602	  chatMessages.value.push({
  1603	    role: "assistant",
  1604	    content: "你好，我是AI智能助手，有什么问题都可以直接问我。"
  1605	  })
  1606	}
  1607	
  1608	
  1609	const selectedWrongId =
  1610	  ref(null)
  1611	
  1612	watch(
  1613	  chatMessages,
  1614	  (value) => {
  1615	    localStorage.setItem("ai_chat_messages", JSON.stringify(value))
  1616	  },
  1617	  { deep: true }
  1618	)
  1619	
  1620	
  1621	
  1622	/* =====================================================
  1623	   当前题目
  1624	   ===================================================== */
  1625	
  1626	const questionData =
  1627	  computed(() => {
  1628	
  1629	    /*
  1630	     * AI智能分析只绑定当前正在作答的题目。
  1631	     * 不读取历史答题记录、错题记录或自动分析缓存。
  1632	     * 切换下一题后，父组件传入的新题目会自动覆盖当前题目。
  1633	     */
  1634	
  1635	    return props.questionData
  1636	
  1637	  })
  1638	
  1639	
  1640	/* =====================================================
  1641	   是否存在当前题目
  1642	   ===================================================== */
  1643	
  1644	const hasQuestion =
  1645	  computed(() => {
  1646	
  1647	    return !!(
  1648	      questionData.value &&
  1649	      questionData.value.question
  1650	    )
  1651	
  1652	  })
  1653	
  1654	
  1655	/* =====================================================
  1656	   题号
  1657	   ===================================================== */
  1658	
  1659	const questionNumber =
  1660	  computed(() => {
  1661	
  1662	    if (
  1663	      !questionData.value
  1664	    ) {
  1665	
  1666	      return 0
  1667	
  1668	    }
  1669	
  1670	
  1671	    if (
  1672	      questionData.value.questionNumber
  1673	    ) {
  1674	
  1675	      return questionData.value.questionNumber
  1676	
  1677	    }
  1678	
  1679	
  1680	    if (
  1681	      typeof questionData.value.questionIndex ===
  1682	      "number"
  1683	    ) {
  1684	
  1685	      return (
  1686	        questionData.value.questionIndex +
  1687	        1
  1688	      )
  1689	
  1690	    }
  1691	
  1692	
  1693	    return 1
  1694	
  1695	  })
  1696	
  1697	
  1698	/* =====================================================
  1699	   选项标准化
  1700	   ===================================================== */
  1701	
  1702	const normalizedOptions =
  1703	  computed(() => {
  1704	
  1705	    if (
  1706	      !questionData.value ||
  1707	      !Array.isArray(
  1708	        questionData.value.options
  1709	      )
  1710	    ) {
  1711	
  1712	      return []
  1713	
  1714	    }
  1715	
  1716	
  1717	    return questionData.value.options.map(
  1718	      (
  1719	        option,
  1720	        index
  1721	      ) => {
  1722	
  1723	        /*
  1724	         * 情况一：
  1725	         * options 是对象
  1726	         *
  1727	         * {
  1728	         *   key: "A",
  1729	         *   text: "xxx"
  1730	         * }
  1731	         */
  1732	
  1733	        if (
  1734	          typeof option ===
  1735	          "object" &&
  1736	          option !== null
  1737	        ) {
  1738	
  1739	          return {
  1740	
  1741	            key:
  1742	              option.key ||
  1743	              String.fromCharCode(
  1744	                65 + index
  1745	              ),
  1746	
  1747	            text:
  1748	              option.text ||
  1749	              ""
  1750	
  1751	          }
  1752	
  1753	        }
  1754	
  1755	
  1756	        /*
  1757	         * 情况二：
  1758	         * options 是字符串
  1759	         *
  1760	         * A. xxx
  1761	         * A、xxx
  1762	         */
  1763	
  1764	        const text =
  1765	          String(option)
  1766	
  1767	
  1768	        const match =
  1769	          text.match(
  1770	            /^([A-Z])[\.\、\s:：]+(.+)$/
  1771	          )
  1772	
  1773	
  1774	        if (match) {
  1775	
  1776	          return {
  1777	
  1778	            key:
  1779	              match[1],
  1780	
  1781	            text:
  1782	              match[2]
  1783	
  1784	          }
  1785	
  1786	        }
  1787	
  1788	
  1789	        return {
  1790	
  1791	          key:
  1792	            String.fromCharCode(
  1793	              65 + index
  1794	            ),
  1795	
  1796	          text
  1797	
  1798	        }
  1799	
  1800	      }
  1801	
  1802	    )
  1803	
  1804	  })
  1805	
  1806	
  1807	/* =====================================================
  1808	   提取答案字母
  1809	   ===================================================== */
  1810	
  1811	const normalizeAnswer =
  1812	  (answer) => {
  1813	
  1814	    if (
  1815	      answer === null ||
  1816	      answer === undefined
  1817	    ) {
  1818	
  1819	      return ""
  1820	
  1821	    }
  1822	
  1823	
  1824	    const value =
  1825	      String(answer)
  1826	        .trim()
  1827	        .toUpperCase()
  1828	
  1829	
  1830	    /*
  1831	     * 直接是 A / B / C / D
  1832	     */
  1833	
  1834	    if (
  1835	      /^[A-Z]$/.test(
  1836	        value
  1837	      )
  1838	    ) {
  1839	
  1840	      return value
  1841	
  1842	    }
  1843	
  1844	
  1845	    /*
  1846	     * A. xxx
  1847	     * A、xxx
  1848	     */
  1849	
  1850	    const match =
  1851	      value.match(
  1852	        /^([A-Z])[\.\、\s:：]/
  1853	      )
  1854	
  1855	
  1856	    if (match) {
  1857	
  1858	      return match[1]
  1859	
  1860	    }
  1861	
  1862	
  1863	    return value
  1864	
  1865	  }
  1866	
  1867	
  1868	/* =====================================================
  1869	   判断选项是否为用户选择
  1870	   ===================================================== */
  1871	
  1872	const isSelectedOption =
  1873	  (option) => {
  1874	
  1875	    if (
  1876	      !questionData.value
  1877	    ) {
  1878	
  1879	      return false
  1880	
  1881	    }
  1882	
  1883	
  1884	    const userAnswer =
  1885	      normalizeAnswer(
  1886	        questionData.value.userAnswer
  1887	      )
  1888	
  1889	
  1890	    return (
  1891	      userAnswer !== "" &&
  1892	      userAnswer ===
  1893	      normalizeAnswer(
  1894	        option.key
  1895	      )
  1896	    )
  1897	
  1898	  }
  1899	
  1900	
  1901	/* =====================================================
  1902	   判断是否正确答案
  1903	   ===================================================== */
  1904	
  1905	const isCorrectOption =
  1906	  (option) => {
  1907	
  1908	    if (
  1909	      !questionData.value
  1910	    ) {
  1911	
  1912	      return false
  1913	
  1914	    }
  1915	
  1916	
  1917	    const correctAnswer =
  1918	      normalizeAnswer(
  1919	        questionData.value.correctAnswer
  1920	      )
  1921	
  1922	
  1923	    return (
  1924	      correctAnswer !== "" &&
  1925	      correctAnswer ===
  1926	      normalizeAnswer(
  1927	        option.key
  1928	      )
  1929	    )
  1930	
  1931	  }
  1932	
  1933	
  1934	/* =====================================================
  1935	   正确答案文字
  1936	   ===================================================== */
  1937	
  1938	const correctOptionText =
  1939	  computed(() => {
  1940	
  1941	    const option =
  1942	      normalizedOptions.value.find(
  1943	        item =>
  1944	          isCorrectOption(item)
  1945	      )
  1946	
  1947	
  1948	    return option
  1949	      ? option.text
  1950	      : "请参考题目正确选项"
  1951	
  1952	  })
  1953	
  1954	
  1955	/* =====================================================
  1956	   答题历史
  1957	   ===================================================== */
  1958	
  1959	const answerHistory =
  1960	  computed(() => {
  1961	
  1962	    if (
  1963	      !Array.isArray(
  1964	        props.answerHistory
  1965	      )
  1966	    ) {
  1967	
  1968	      return []
  1969	
  1970	    }
  1971	
  1972	
  1973	    return props.answerHistory
  1974	
  1975	  })
  1976	
  1977	
  1978	/* =====================================================
  1979	   错题本
  1980	   ===================================================== */
  1981	
  1982	const wrongAnswers =
  1983	  computed(() => {
  1984	
  1985	    return answerHistory.value.filter(
  1986	      record =>
  1987	        record &&
  1988	        record.correct === false
  1989	    )
  1990	
  1991	  })
  1992	
  1993	
  1994	/* =====================================================
  1995	   总答题数
  1996	   ===================================================== */
  1997	
  1998	const totalAnswers =
  1999	  computed(() => {
  2000	
