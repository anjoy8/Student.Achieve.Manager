import axios from 'axios';
// import router from '../routerManuaConfig'
import router from '../router/index'
import store from "../store";
import Vue from 'vue';

// 如果是iis部署的话，这么写，因为要使用后端api的绝对路径
//let base = process.env.NODE_ENV=="production"? 'http://localhost:6919':'';
// 如果是nginx的话，就置空就行了，因为用的是代理模式
let base = '';

// 请求延时
axios.defaults.timeout = 50000

var storeTemp = store;
axios.interceptors.request.use(
    config => {
        var curTime = new Date()
        var expiretime = new Date(Date.parse(storeTemp.state.tokenExpire))

        if (storeTemp.state.token && (curTime < expiretime && storeTemp.state.tokenExpire)) {
            // 判断是否存在token，如果存在的话，则每个http header都加上token
            config.headers.Authorization = "Bearer " + storeTemp.state.token;
        }

        saveRefreshtime();

        return config;
    },
    err => {
        return Promise.reject(err);
    }
);

// http response 拦截器
axios.interceptors.response.use(
    response => {
        return response;
    },
    error => {
        // 超时请求处理
        var originalRequest = error.config;
        if(error.code == 'ECONNABORTED' && error.message.indexOf('timeout')!=-1 && !originalRequest._retry){

            Vue.prototype.$message({
                message: '请求超时！',
                type: 'error'
            });

            originalRequest._retry = true
            return null;
        }

        if (error.response) {
            if (error.response.status == 401) {
                var curTime = new Date()
                var refreshtime = new Date(Date.parse(window.localStorage.refreshtime))
                // 在用户操作的活跃期内
                if (window.localStorage.refreshtime && (curTime <= refreshtime)) {
                    return  refreshToken({token: window.localStorage.Token}).then((res) => {
                        if (res.success) {
                            Vue.prototype.$message({
                                message: 'refreshToken success! loading data...',
                                type: 'success'
                            });

                            store.commit("saveToken", res.token);

                            var curTime = new Date();
                            var expiredate = new Date(curTime.setSeconds(curTime.getSeconds() + res.expires_in));
                            store.commit("saveTokenExpire", expiredate);

                            error.config.__isRetryRequest = true;
                            error.config.headers.Authorization = 'Bearer ' + res.token;
                            return axios(error.config);
                        } else {
                            // 刷新token失败 清除token信息并跳转到登录页面
                            ToLogin()
                        }
                    });
                } else {
                    // 返回 401，并且不知用户操作活跃期内 清除token信息并跳转到登录页面
                    ToLogin()
                }

            }
            // 403 无权限
            if (error.response.status == 403) {
                Vue.prototype.$message({
                    message: '失败！该操作无权限',
                    type: 'error'
                });
                return null;
            }
        }
        return ""; // 返回接口返回的错误信息
    }
);

// 登录
export const requestLogin = params => {
    return axios.get(`${base}/api/login/jwttoken3.0`, {params: params}).then(res => res.data);
};
export const requestLoginMock = params => { return axios.post(`${base}/login`, params).then(res => res.data); };

export const refreshToken = params => {
    return axios.get(`${base}/api/login/RefreshToken`, {params: params}).then(res => res.data);
};

export const saveRefreshtime = params => {

    let nowtime = new Date();
    let lastRefreshtime = window.localStorage.refreshtime ? new Date(window.localStorage.refreshtime) : new Date(-1);
    let expiretime = new Date(Date.parse(window.localStorage.TokenExpire))

    let refreshCount=1;//滑动系数
    if (lastRefreshtime >= nowtime) {
        lastRefreshtime=nowtime>expiretime ? nowtime:expiretime;
        lastRefreshtime.setMinutes(lastRefreshtime.getMinutes() + refreshCount);
        window.localStorage.refreshtime = lastRefreshtime;
    }else {
        window.localStorage.refreshtime = new Date(-1);
    }
};
 const ToLogin = params => {
     store.commit("saveToken", "");
     store.commit("saveTokenExpire", "");
     store.commit("saveTagsData", "");
     window.localStorage.removeItem('user');
     window.localStorage.removeItem('NavigationBar');

     router.replace({
         path: "/login",
         query: {redirect: router.currentRoute.fullPath}
     });

      window.location.reload()

};

export const getUserByToken = params => {
    return axios.get(`${base}/api/user/getInfoByToken`, {params: params}).then(res => res.data);
};


export function testapi2() {
    console.log('api is ok.')
}

export const testapi = pa => {
    console.log('api is ok.')
}

// 用户管理
export const getUserListPage = params => {
    return axios.get(`${base}/api/user/get`, {params: params});
};
export const removeUser = params => {
    return axios.delete(`${base}/api/user/delete`, {params: params});
};
export const editUser = params => {
    return axios.put(`${base}/api/user/put`, params);
};
export const addUser = params => {
    return axios.post(`${base}/api/user/post`, params);
};
export const batchRemoveUser = params => {
    return axios.delete(`${base}/api/Claims/BatchDelete`, {params: params});//没做
};

// 角色管理
export const getRoleListPage = params => {
    return axios.get(`${base}/api/role/get`, {params: params});
};
export const removeRole = params => {
    return axios.delete(`${base}/api/role/delete`, {params: params});
};
export const editRole = params => {
    return axios.put(`${base}/api/role/put`, params);
};
export const addRole = params => {
    return axios.post(`${base}/api/role/post`, params);
};

// 接口模块管理
export const getModuleListPage = params => {
    return axios.get(`${base}/api/module/get`, {params: params});
};
export const removeModule = params => {
    return axios.delete(`${base}/api/module/delete`, {params: params});
};
export const editModule = params => {
    return axios.put(`${base}/api/module/put`, params);
};
export const addModule = params => {
    return axios.post(`${base}/api/module/post`, params);
};


// 菜单模块管理
export const getPermissionListPage = params => {
    return axios.get(`${base}/api/permission/get`, {params: params});
};
export const removePermission = params => {
    return axios.delete(`${base}/api/permission/delete`, {params: params});
};
export const editPermission = params => {
    return axios.put(`${base}/api/permission/put`, params);
};
export const addPermission = params => {
    return axios.post(`${base}/api/permission/post`, params);
};
export const getPermissionTree = params => {
    return axios.get(`${base}/api/permission/getpermissiontree`, {params: params});
};
export const getPermissionIds = params => {
    return axios.get(`${base}/api/permission/GetPermissionIdByRoleId`, {params: params});
};

export const addRolePermission = params => {
    return axios.post(`${base}/api/permission/Assign`, params);
};
export const getNavigationBar = params => {
    return axios.get(`${base}/api/permission/GetNavigationBar`, {params: params}).then(res => res.data);
};

// Bug模块管理
export const getBugListPage = params => {
    return axios.get(`${base}/api/TopicDetail/get`, {params: params});
};
export const removeBug = params => {
    return axios.delete(`${base}/api/TopicDetail/delete`, {params: params});
};
export const editBug = params => {
    return axios.put(`${base}/api/TopicDetail/update`, params);
};
export const addBug = params => {
    return axios.post(`${base}/api/TopicDetail/post`, params);
};



// Grade
export const addGrade = params => {
    return axios.post(`${base}/api/Grade/post`, params);
};
export const getGradeListPage = params => {
    return axios.get(`${base}/api/Grade/get`, {params: params});
};
export const removeGrade = params => {
    return axios.delete(`${base}/api/Grade/delete`, {params: params});
};
export const editGrade = params => {
    return axios.put(`${base}/api/Grade/put`, params);
};
export const getGradeTree = params => {
    return axios.get(`${base}/api/Grade/GetGradeTree`, {params: params});
};
// Clazz
export const addClazz = params => {
    return axios.post(`${base}/api/Clazz/post`, params);
};
export const getClazzListPage = params => {
    return axios.get(`${base}/api/Clazz/get`, {params: params});
};
export const removeClazz = params => {
    return axios.delete(`${base}/api/Clazz/delete`, {params: params});
};
export const editClazz = params => {
    return axios.put(`${base}/api/Clazz/put`, params);
};
export const getClazzTree = params => {
    return axios.get(`${base}/api/Clazz/GetClazzTree`, {params: params});
};
// Course
export const addCourse = params => {
    return axios.post(`${base}/api/Course/post`, params);
};
export const getCourseListPage = params => {
    return axios.get(`${base}/api/Course/get`, {params: params});
};
export const removeCourse = params => {
    return axios.delete(`${base}/api/Course/delete`, {params: params});
};
export const editCourse = params => {
    return axios.put(`${base}/api/Course/put`, params);
};
export const getCourseTree = params => {
    return axios.get(`${base}/api/Course/GetCourseTree`, {params: params});
};
// Teacher
export const addTeacher = params => {
    return axios.post(`${base}/api/Teacher/post`, params);
};
export const getTeacherListPage = params => {
    return axios.get(`${base}/api/Teacher/get`, {params: params});
};
export const removeTeacher = params => {
    return axios.delete(`${base}/api/Teacher/delete`, {params: params});
};
export const editTeacher = params => {
    return axios.put(`${base}/api/Teacher/put`, params);
};
// Students
export const addStudents = params => {
    return axios.post(`${base}/api/Students/post`, params);
};
export const getStudentsListPage = params => {
    return axios.get(`${base}/api/Students/get`, {params: params});
};
export const removeStudents = params => {
    return axios.delete(`${base}/api/Students/delete`, {params: params});
};
export const editStudents = params => {
    return axios.put(`${base}/api/Students/put`, params);
};
export const getStudentsTree = params => {
    return axios.get(`${base}/api/Students/GetStudentsTree`, {params: params});
};
// Exam
export const addExam = params => {
    return axios.post(`${base}/api/Exam/post`, params);
};
export const getExamListPage = params => {
    return axios.get(`${base}/api/Exam/get`, {params: params});
};
export const removeExam = params => {
    return axios.delete(`${base}/api/Exam/delete`, {params: params});
};
export const editExam = params => {
    return axios.put(`${base}/api/Exam/put`, params);
};
export const getExamTree = params => {
    return axios.get(`${base}/api/Exam/GetExamTree`, {params: params});
};
// ExScore
export const addExScore = params => {
    return axios.post(`${base}/api/ExScore/post`, params);
};
export const getExScoreListPage = params => {
    return axios.get(`${base}/api/ExScore/get`, {params: params});
};
export const removeExScore = params => {
    return axios.delete(`${base}/api/ExScore/delete`, {params: params});
};
export const editExScore = params => {
    return axios.put(`${base}/api/ExScore/put`, params);
};

// ExamDetail
export const addExamDetail = params => {
    return axios.post(`${base}/api/ExamDetail/post`, params);
};
export const getExamDetailListPage = params => {
    return axios.get(`${base}/api/ExamDetail/get`, {params: params});
};
export const removeExamDetail = params => {
    return axios.delete(`${base}/api/ExamDetail/delete`, {params: params});
};
export const editExamDetail = params => {
    return axios.put(`${base}/api/ExamDetail/put`, params);
};
// ExamDetailScore
export const addExamDetailScore = params => {
    return axios.post(`${base}/api/ExamDetailScore/post`, params);
};
export const getExamDetailScoreListPage = params => {
    return axios.get(`${base}/api/ExamDetailScore/get`, {params: params});
};
export const removeExamDetailScore = params => {
    return axios.delete(`${base}/api/ExamDetailScore/delete`, {params: params});
};
export const editExamDetailScore = params => {
    return axios.put(`${base}/api/ExamDetailScore/put`, params);
};
// CourseScoreDistribute
export const getCourseScoreDistributeListPage = params => {
    return axios.get(`${base}/api/CourseScoreDistribute/get`, {params: params});
};
export const getExamTreeYearTerm = params => {
    return axios.get(`${base}/api/Exam/GetExamTreeYearTerm`, {params: params});
};
export const getExamTreeExam = params => {
    return axios.get(`${base}/api/Exam/GetExamTreeExam`, {params: params});
};
export const getSingleCourseListPage = params => {
    return axios.get(`${base}/api/SingleCourse/get`, {params: params});
};
export const getObjective = params => {
    return axios.get(`${base}/api/Objective/get`, {params: params});
};
export const getSubjective = params => {
    return axios.get(`${base}/api/Subjective/get`, {params: params});
};
export const getFSN = params => {
    return axios.get(`${base}/api/FSN/get`, {params: params});
};
export const getPositivePointListPage = params => {
    return axios.get(`${base}/api/PositivePoint/get`, {params: params});
};







export const getSingleCourseStudentListPage = params => {
    return axios.get(`${base}/api/SingleCourseStudent/get`, {params: params});
};
export const getFSNStudent = params => {
    return axios.get(`${base}/api/FSNStudent/get`, {params: params});
};
export const getObjectiveStudent = params => {
    return axios.get(`${base}/api/ObjectiveStudent/get`, {params: params});
};
