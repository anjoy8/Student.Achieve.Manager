<template>
  <section>
    <el-menu
      class="el-menu-demo"
      default-active="/Student/SingleCourse"
      mode="horizontal"
      background-color="#545c64"
      text-color="#fff"
      router
      active-text-color="#ffd04b"
    >
      <el-menu-item index="/Student/SingleCourse">单科成绩</el-menu-item>
      <el-menu-item index="/Student/Objective">客观题分析</el-menu-item>
      <el-menu-item index="/Student/Summary">成绩汇总</el-menu-item>
     
    </el-menu>

    <!--工具条-->
    <el-col :span="24" class="toolbar" style="padding-bottom: 0px;">
      <el-form :inline="true" :model="filters" @submit.native.prevent>
        <el-form-item>
          <el-select @change="getExamTreeFunc" v-model="GradeId" placeholder="请选择">
            <el-option
              v-for="item in GradeTree"
              :key="item.value"
              :label="item.label"
              :value="item.value"
            ></el-option>
          </el-select>

          <el-select v-model="ClazzId" style="width:100px;" placeholder="请选择">
            <el-option
              v-for="item in ClazzTree"
              :key="item.value"
              :label="item.label"
              :value="item.value"
            ></el-option>
          </el-select>
          <el-select v-model="AcademicYearSchoolTerm" placeholder="请选择">
            <el-option
              v-for="item in AcademicYearSchoolTermTree"
              :key="item.value"
              :label="item.label"
              :value="item.value"
            ></el-option>
          </el-select>
          <el-select v-model="ExamName" style="width:150px;" placeholder="请选择">
            <el-option
              v-for="item in ExamNameTree"
              :key="item.value"
              :label="item.label"
              :value="item.value"
            ></el-option>
          </el-select>
          <el-select v-model="courseid" style="width:150px;" placeholder="请选择">
            <el-option
              v-for="item in courseidTree"
              :key="item.value"
              :label="item.label"
              :value="item.value"
            ></el-option>
          </el-select>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="getSingleCourseStudent">查询</el-button>
        </el-form-item>
      </el-form>
    </el-col>

    <!--列表-->
     <el-table 
     :data="SingleCourseStudent"  
     highlight-current-row
      v-loading="listLoading"
      :height="screentheight"
      @selection-change="selsChange"
      style="width: 100%;">
      <el-table-column v-if="cols.length>0" type="index"></el-table-column>
      <el-table-column 
        v-for="col in cols"
        :prop="col.prop" :label="col.label" >
      </el-table-column>
    </el-table>
    <!--工具条-->
    <el-col :span="24" class="toolbar">
      <el-pagination
        layout="prev, pager, next"
        @current-change="nextPageSingleCourseStudent"
        :page-size="100"
        :total="total"
        style="float:right;"
      ></el-pagination>
    </el-col>
  </section>
</template>

<script>
import util from "../../../util/date";
import {
  getSingleCourseStudentListPage,
  getGradeTree,
  getExamTreeYearTerm,
  getExamTreeExam,
  getCourseTree,
  getClazzTree
} from "../../api/api";

export default {
  data() {
    return {
      filters: {
        name: ""
      },
      GradeId: "",
      screentheight: 300,
      cols:[],
      SingleCourseStudent: [],
      AcademicYearSchoolTermTree: [],
      ExamName: "",
      AcademicYearSchoolTerm: "",
      ExamNameTree: [],
      GradeTree: [],
      courseid: "",
      courseidTree: [],
      ClazzId: "",
      ClazzTree: [],
      total: 0,
      page: 1,
      listLoading: false,
      routes: [],
      sels: [] //列表选中列
    };
  },
  methods: {
      handleSelect(key, keyPath) {
        console.log(key, keyPath);
      },
    formatSex: function(row, column) {
      return row.sex == 1 ? "男" : row.sex == 0 ? "女" : "未知";
    },
    formatBirth: function(row, column) {
      return !row.birth || row.birth == ""
        ? ""
        : util.formatDate.format(new Date(row.birth), "yyyy-MM-dd");
    },
    nextPageSingleCourseStudent(val) {
      this.page = val;
      this.getSingleCourseStudent();
    },
    getExamTreeFunc() {
      this.AcademicYearSchoolTerm = "";
      this.ExamName = "";

      let para = { gid: this.GradeId };
      getExamTreeYearTerm(para).then(res => {
        this.AcademicYearSchoolTermTree = res.data.response;
      });
      getExamTreeExam(para).then(res => {
        this.ExamNameTree = res.data.response;
      });
      getClazzTree(para).then(res => {
        this.ClazzTree = res.data.response;
      this.ClazzId = this.ClazzTree ? this.ClazzTree[0].value : "";

      });
    },
    //获取用户列表
    getSingleCourseStudent() {
      let para = {
        page: this.page,
        key: this.filters.name,
        GradeId: this.GradeId || 0,
        AcademicYearSchoolTerm: this.AcademicYearSchoolTerm,
        ExamName: this.ExamName,
        CourseId: this.courseid || 0,
        ClazzId: this.ClazzId || 0
      };
      this.listLoading = true;

      getSingleCourseStudentListPage(para).then(res => {
        this.cols =JSON.parse(res.data.response.Header);
        this.SingleCourseStudent = JSON.parse(res.data.response.Content);
        this.listLoading = false;
      });
    },

    selsChange: function(sels) {
      this.sels = sels;
    }
  },
  created() {
    const that = this;
    window.screentheight = document.body.clientHeight;
    that.screentheight = window.screentheight - 250;
    window.onresize = () => {
      return (() => {
        window.screentheight = document.body.clientHeight;
        that.screentheight = window.screentheight - 250;
      })();
    };

    this.routes = [
      {
        id: 89,
        pid: 88,
        order: 0,
        name: "单科成绩",
        IsHide: false,
        path: "/Analysis/SingleCourseStudent",
        iconCls: "",
        meta: {
          title: "单科成绩",
          requireAuth: true,
          NoTabPage: false
        },
        children: null
      },
      {
        id: 90,
        pid: 88,
        order: 0,
        name: "各班客观题",
        IsHide: false,
        path: "/Analysis/Objective",
        iconCls: "",
        meta: {
          title: "各班客观题",
          requireAuth: true,
          NoTabPage: false
        },
        children: null
      },
      {
        id: 91,
        pid: 88,
        order: 0,
        name: "各班主观题",
        IsHide: false,
        path: "/Analysis/Subjective",
        iconCls: "",
        meta: {
          title: "各班主观题",
          requireAuth: true,
          NoTabPage: false
        },
        children: null
      },
      {
        id: 92,
        pid: 88,
        order: 0,
        name: "各科成绩分布",
        IsHide: false,
        path: "/Analysis/CourseScoreDistribute",
        iconCls: "",
        meta: {
          title: "各科成绩分布",
          requireAuth: true,
          NoTabPage: false
        },
        children: null
      }
    ];
    // window.localStorage.setItem("router", JSON.stringify(roterJson));
  },
  destroyed() {
    window.localStorage.removeItem("router");
  },
  mounted() {
    // this.getSingleCourseStudent();

    let para = {};
    getGradeTree(para)
      .then(res => {
        this.GradeTree = res.data.response;
        this.GradeId = this.GradeTree ? this.GradeTree[0].value : "";
      })
      .then(res => {
        this.getExamTreeFunc();
      });
    getCourseTree(para).then(res => {
      this.courseidTree = res.data.response;
      this.courseid = this.courseidTree ? this.courseidTree[0].value : "";
    });
  }
};
</script>

<style scoped>
</style>
