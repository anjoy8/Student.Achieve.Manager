FROM nginx
MAINTAINER vueadmin
ADD dist /usr/share/nginx/html
ADD student.nginx.conf /etc/nginx/nginx.conf
RUN chown nginx:nginx -R /usr/share/nginx/html
EXPOSE 80
RUN echo 'build student image successful!!'
