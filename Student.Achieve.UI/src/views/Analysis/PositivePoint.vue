<template>
  <section>
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
 
            <!-- <el-select v-model="ClazzId" style="width:100px;" placeholder="请选择">
              <el-option
                v-for="item in ClazzTree"
                :key="item.value"
                :label="item.label"
                :value="item.value"
              ></el-option>
            </el-select>  -->
          <el-select v-model="AcademicYearSchoolTerm" placeholder="请选择">
            <el-option
              v-for="item in AcademicYearSchoolTermTree"
              :key="item.value"
              :label="item.label"
              :value="item.value"
            ></el-option>
          </el-select>
          <el-select v-model="ExamName"  style="width:150px;" placeholder="请选择">
            <el-option
              v-for="item in ExamNameTree"
              :key="item.value"
              :label="item.label"
              :value="item.value"
            ></el-option>
          </el-select>
          <el-select v-model="courseid"  style="width:150px;" placeholder="请选择">
            <el-option
              v-for="item in courseidTree"
              :key="item.value"
              :label="item.label"
              :value="item.value"
            ></el-option>
          </el-select>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="getPositivePoint">查询</el-button>
        </el-form-item>
      </el-form>
    </el-col>

    <!--列表-->
    <el-table
      :data="PositivePoint"
      highlight-current-row
      v-loading="listLoading"
      :height="screentheight"
      @selection-change="selsChange"
      :row-class-name="tableRowClassName"
      style="width: 100%;"
    >
      <el-table-column type="index" width="50"></el-table-column>
      <el-table-column prop="Clazz" label="班别" width="" sortable></el-table-column>
      <el-table-column prop="Teacher" label="老师" width="" sortable></el-table-column>
      <el-table-column prop="TotalStudentCount" label="基础人数" width="" sortable></el-table-column>
      <el-table-column prop="ExamStudentCount" label="参考人数" width="" sortable></el-table-column>
      <el-table-column prop="NoExamStudentCount" label="缺考人数" width="" sortable></el-table-column>
      <el-table-column prop="ThisBaseOk" label="本期基础" width="" sortable></el-table-column>
      <el-table-column prop="ThisExamOk" label="本次实考" width="" sortable></el-table-column>
      <el-table-column prop="ThisOk" label="本期得分" width="" sortable></el-table-column>
      <el-table-column prop="RankBase" label="比基础" width="" sortable></el-table-column>
    </el-table>
    <!--工具条-->
    <el-col :span="24" class="toolbar">
      <el-pagination
        layout="prev, pager, next"
        @current-change="nextPagePositivePoint"
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
  getPositivePointListPage,
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
      PositivePoint: [],
      AcademicYearSchoolTermTree: [],
      ExamName: "",
      AcademicYearSchoolTerm: "",
      ExamNameTree: [],
      GradeTree: [],
      courseid: "",
      courseidTree: [],
      ClazzId:"",
      ClazzTree:[],
      total: 0,
      page: 1,
      listLoading: false,
      sels: [] //列表选中列
    };
  },
  methods: {
       tableRowClassName({row, rowIndex}) {
        if (row.RankBase < 0) {
          return 'warning-row';
        } else if (rowIndex === 3) {
          return 'success-row';
        }
        return '';
      },
    formatSex: function(row, column) {
      return row.sex == 1 ? "男" : row.sex == 0 ? "女" : "未知";
    },
    formatBirth: function(row, column) {
      return !row.birth || row.birth == ""
        ? ""
        : util.formatDate.format(new Date(row.birth), "yyyy-MM-dd");
    },
    nextPagePositivePoint(val) {
      this.page = val;
      this.getPositivePoint();
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
    getPositivePoint() {
      let para = {
        page: this.page,
        key: this.filters.name,
        GradeId: this.GradeId || 0,
        AcademicYearSchoolTerm: this.AcademicYearSchoolTerm,
        ExamName: this.ExamName,
        CourseId: this.courseid || 0,
        ClazzId:this.ClazzId||0
      };
      this.listLoading = true;

      getPositivePointListPage(para).then(res => {
        this.total = res.data.response.length;
        this.PositivePoint = res.data.response;
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
  },
  mounted() {
    // this.getPositivePoint();

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

<style>
  .el-table .warning-row {
    background: oldlace;
  }

  .el-table .success-row {
    background: #f0f9eb;
  }
</style>