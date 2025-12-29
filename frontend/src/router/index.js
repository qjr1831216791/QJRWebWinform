import Vue from 'vue'
import Router from 'vue-router'

Vue.use(Router)

const router = new Router({
  mode: 'hash', // 桌面应用推荐使用 hash 模式
  base: process.env.BASE_URL,
  routes: [
    {
      path: '/qjrweb',
      name: 'Home',
      component: () => import('@/components/Home/Home.vue'),
      children: [
        {
          path: '/SelfIntroduction',
          name: 'SelfIntroduction',
          component: () => import('@/components/Common/SelfIntroduction/SelfIntroduction.vue')
        },
        {
          path: "ConfigurationSync/main",
          meta: {
            allowAnonymous: true,
          },
          component: () => import('@/components/ConfigurationSync/ConfigurationSync.vue'),
        },
        {
          path: "Test/TestPage",
          meta: {
            allowAnonymous: true,
          },
          component: () => import('@/components/Test/TestPage.vue')
        },
        {
          path: "CRMEntityMetadata/main",
          meta: {
            allowAnonymous: true,
          },
          component: () => import('@/components/CRMEntityMetadata/CRMEntityMetadata.vue')
        },
        {
          path: "WeatherForecast/main",
          meta: {
            allowAnonymous: true,
          },
          component: () => import('@/components/WeatherForecast/WeatherForecast.vue')
        },
        {
          path: "CRMUserAndRoles/main",
          meta: {
            allowAnonymous: true,
          },
          component: () => import('@/components/CRMUserAndRoles/CRMUserAndRoles.vue')
        },
        {
          path: "CRMTreeGraph/main",
          meta: {
            allowAnonymous: true,
          },
          component: () => import('@/components/CRMTreeGraph/CRMTreeGraph.vue')
        },
        {
          path: "CRMPluginTraceLog/main",
          meta: {
            allowAnonymous: true,
          },
          component: () => import('@/components/CRMPluginTraceLog/CRMPluginTraceLog.vue')
        },
        {
          path: "CRMFetchXmlQuery/main",
          meta: {
            allowAnonymous: true,
          },
          component: () => import('@/components/CRMFetchXmlQuery/CRMFetchXmlQuery.vue')
        },
      ]
    },
    {
      path: '*',
      name: 'not-found',
      redirect: { name: 'Home' }  // 当无法识别的路径被访问时，重定向到home路由
    },
    {
      path: '/ConfigurationSync',
      name: 'ConfigurationSync',
      component: () => import('@/components/ConfigurationSync/ConfigurationSync.vue')
    },
    {
      path: '/Test/TestPage',
      name: 'TestPage',
      component: () => import('@/components/Test/TestPage.vue')
    },
    {
      path: '/CRMEntityMetadata',
      name: 'CRMEntityMetadata',
      component: () => import('@/components/CRMEntityMetadata/CRMEntityMetadata.vue')
    },
    {
      path: '/WeatherForecast',
      name: 'WeatherForecast',
      component: () => import('@/components/WeatherForecast/WeatherForecast.vue')
    },
    {
      path: '/CRMUserAndRoles',
      name: 'CRMUserAndRoles',
      component: () => import('@/components/CRMUserAndRoles/CRMUserAndRoles.vue')
    },
    {
      path: '/CRMTreeGraph',
      name: 'CRMTreeGraph',
      component: () => import('@/components/CRMTreeGraph/CRMTreeGraph.vue')
    },
    {
      path: '/CRMPluginTraceLog',
      name: 'CRMPluginTraceLog',
      component: () => import('@/components/CRMPluginTraceLog/CRMPluginTraceLog.vue')
    },
    {
      path: '/CRMFetchXmlQuery',
      name: 'CRMFetchXmlQuery',
      component: () => import('@/components/CRMFetchXmlQuery/CRMFetchXmlQuery.vue')
    },
  ]
})

router.beforeEach((to, from, next) => {
  if (from.path !== to.path) {
    next() // 只有当前路由不是目标路由时才继续
  }
})

export default router

