import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  title = 'Kishore app!!';
  users: any;

  constructor(private http: HttpClient){}
  ngOnInit(): void {
    this.http.get('http://localhost:5144/api/users').subscribe({
      next: response => this.users = response,
      error: error => console.log(error),
      complete: () => console.log('request is completed ')
    })
  }
}
