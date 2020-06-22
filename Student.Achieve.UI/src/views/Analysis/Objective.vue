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
          <el-button type="primary" @click="getObjective">查询</el-button>
        </el-form-item>
      </el-form>
    </el-col>

    <!--列表-->
    <el-table
      :data="Objective"
      highlight-current-row
      v-loading="listLoading"
      :height="screentheight"
      @selection-change="selsChange"
      style="width: 100%;"
    >
      <el-table-column type="index" width="50"></el-table-column>
      <el-table-column prop="Name" label="题号" width="100" sortable></el-table-column>
      <el-table-column prop="Answer" label="答案" width="100" sortable></el-table-column>
      <el-table-column prop="A" label="A" width="100" sortable></el-table-column>
      <el-table-column prop="B" label="B" width="100" sortable></el-table-column>
      <el-table-column prop="C" label="C" width="100" sortable></el-table-column>
      <el-table-column prop="D" label="D" width="100" sortable></el-table-column>
      <el-table-column prop="AB" label="AB" width="100" sortable></el-table-column>
      <el-table-column prop="ABC" label="ABC" width="100" sortable></el-table-column>
      <el-table-column prop="ABCD" label="ABCD" width="100" sortable></el-table-column>
      <el-table-column prop="AC" label="AC" width="100" sortable></el-table-column>
      <el-table-column prop="ACD" label="ACD" width="100" sortable></el-table-column>
      <el-table-column prop="AD" label="AD" width="100" sortable></el-table-column>
      <el-table-column prop="BC" label="BC" width="100" sortable></el-table-column>
      <el-table-column prop="BCD" label="BCD" width="100" sortable></el-table-column>
      <el-table-column prop="BD" label="BD" width="100" sortable></el-table-column>
      <el-table-column prop="CD" label="CD" width="100" sortable></el-table-column>
      <el-table-column prop="NoWrite" label="未涂" width="100" sortable></el-table-column>
      <el-table-column prop="ReadCardCount" label="读卡" width="100" sortable></el-table-column>
      <el-table-column prop="StudentTotalScore" label="得分" width="100" sortable></el-table-column>
      <el-table-column prop="TotalScore" label="总分" width="100" sortable></el-table-column>

      <el-table-column label="得分率" width="100" sortable>
        <template scope="scope">{{scope.row.TotalScoreRate}}%</template>
      </el-table-column>
    </el-table>
    <!--工具条-->
    <el-col :span="24" class="toolbar">
      <el-pagination
        layout="prev, pager, next"
        @current-change="nextPageObjective"
        :page-size="50"
        :total="total"
        style="float:right;"
      ></el-pagination>
    </el-col>
  </section>
</template>

<script>
import util from "../../../util/date";
import {
  getObjective,
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
      Objective:[],
       GradeId: "",
      screentheight: 300,
      SingleCourse: [],
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
    formatSex: function(row, column) {
      return row.sex == 1 ? "男" : row.sex == 0 ? "女" : "未知";
    },
    formatBirth: function(row, column) {
      return !row.birth || row.birth == ""
        ? ""
        : util.formatDate.format(new Date(row.birth), "yyyy-MM-dd");
    },
    nextPageObjective(val) {
      this.page = val;
      this.getObjective();
    },
    getExamTreeFunc() {
      this.AcademicYearSchoolTerm = "";
      this.ExamName = "";
      let para = { gid: this.GradeId };
      getExamTreeYearTerm(para).then(res => {
      console.log(res.data.response)
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
    getObjective() {
      let para = {
        page: this.page,
        key: this.filters.name,
        GradeId: this.GradeId || 0,
        AcademicYearSchoolTerm: this.AcademicYearSchoolTerm,
        ExamName: this.ExamName,
        ClazzId:this.ClazzId||0,
        CourseId: this.courseid || 0
      };
      this.listLoading = true;

      getObjective(para).then(res => {
        this.total = res.data.response.dataCount;
        this.Objective = res.data.response.data;
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
    // this.getObjective();

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
