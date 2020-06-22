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
          <el-select v-model="AcademicYearSchoolTerm" placeholder="请选择">
            <el-option
              v-for="item in AcademicYearSchoolTermTree"
              :key="item.value"
              :label="item.label"
              :value="item.value"
            ></el-option>
          </el-select>
          <el-select v-model="ExamName" placeholder="请选择">
            <el-option
              v-for="item in ExamNameTree"
              :key="item.value"
              :label="item.label"
              :value="item.value"
            ></el-option>
          </el-select>
          <el-select v-model="courseid" placeholder="请选择">
            <el-option
              v-for="item in courseidTree"
              :key="item.value"
              :label="item.label"
              :value="item.value"
            ></el-option>
          </el-select>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="getCourseScoreDistribute">查询</el-button>
        </el-form-item>
      </el-form>
    </el-col>

    <!--列表-->
    <el-table
      :data="CourseScoreDistribute"
      highlight-current-row
      v-loading="listLoading"
      :height="screentheight"
      @selection-change="selsChange"
      style="width: 100%;"
    >
      <el-table-column type="index" width="50"></el-table-column>
      <el-table-column prop="Clazz" label="班级" width="120" sortable></el-table-column>
      <el-table-column prop="Teacher" label="老师" width="120" sortable></el-table-column>
      <el-table-column prop="ExamStuCount" label="考生数" width="120" sortable></el-table-column>
      <el-table-column prop="C140_150" label="140_150" width="120" sortable></el-table-column>
      <el-table-column prop="C130_139" label="130_139" width="120" sortable></el-table-column>
      <el-table-column prop="C120_129" label="120_129" width="120" sortable></el-table-column>
      <el-table-column prop="C110_119" label="110_119" width="120" sortable></el-table-column>
      <el-table-column prop="C100_109" label="100_109" width="120" sortable></el-table-column>
      <el-table-column prop="C90_99" label="90_99" width="120" sortable></el-table-column>
      <el-table-column prop="C80_89" label="80_89" width="120" sortable></el-table-column>
      <el-table-column prop="C70_79" label="70_79" width="120" sortable></el-table-column>
      <el-table-column prop="C60_69" label="60_69" width="120" sortable></el-table-column>
      <el-table-column prop="C50_59" label="50_59" width="120" sortable></el-table-column>
      <el-table-column prop="C40_49" label="40_49" width="120" sortable></el-table-column>
      <el-table-column prop="C40_0" label="40_0" width="120" sortable></el-table-column>
      <el-table-column prop="C_Max" label="最高分" width="120" sortable></el-table-column>
      <el-table-column prop="C_Min" label="最低分" width="120" sortable></el-table-column>
      <el-table-column prop="C_Avg" label="平均分" width="120" sortable></el-table-column>
      <el-table-column prop="C_Good" label="优秀数" width="120" sortable></el-table-column>
      <el-table-column prop="C_Pass" label="通过数" width="120" sortable></el-table-column>
      <el-table-column label="优秀率" width="120" sortable>
        <template scope="scope">{{scope.row.C_Good_Rate}}%</template>
      </el-table-column>
      <el-table-column label="通过率" width="120" sortable>
        <template scope="scope">{{scope.row.C_Pass_Rate}}%</template>
      </el-table-column>
    </el-table>
    <!--工具条-->
    <el-col :span="24" class="toolbar">
      <el-pagination
        layout="prev, pager, next"
        @current-change="nextPageCourseScoreDistribute"
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
  getCourseScoreDistributeListPage,
  getGradeTree,
  getExamTreeYearTerm,
  getExamTreeExam,
  getCourseTree
} from "../../api/api";

export default {
  data() {
    return {
      filters: {
        name: ""
      },
      GradeId: "",
      screentheight: 300,
      CourseScoreDistribute: [],
      AcademicYearSchoolTermTree: [],
      ExamName: "",
      AcademicYearSchoolTerm: "",
      ExamNameTree: [],
      GradeTree: [],
      courseid: "",
      courseidTree: [],
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
    nextPageCourseScoreDistribute(val) {
      this.page = val;
      this.getCourseScoreDistribute();
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
    },
    //获取用户列表
    getCourseScoreDistribute() {
      let para = {
        page: this.page,
        key: this.filters.name,
        GradeId: this.GradeId || 0,
        AcademicYearSchoolTerm: this.AcademicYearSchoolTerm,
        ExamName: this.ExamName,
        CourseId: this.courseid || 0
      };
      this.listLoading = true;

      getCourseScoreDistributeListPage(para).then(res => {
        this.total = res.data.response.dataCount;
        this.CourseScoreDistribute = res.data.response.data;
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
    // this.getCourseScoreDistribute();

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
